#region LGPL License
/*
Jad Engine Library
Copyright (C) 2007 Jad Engine Project Team

This library is free software; you can redistribute it and/or
modify it under the terms of the GNU Lesser General Public
License as published by the Free Software Foundation; either
version 2.1 of the License, or (at your option) any later version.

This library is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public
License along with this library; if not, write to the Free Software
Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Linq;

using Supremacy.VFS.Utilities;

#endregion

namespace Supremacy.VFS
{
	///<summary>
	/// An abstract base implementation of <see cref="IFilesSource"/>.
	///</summary>
	///<typeparam name="T">The type of stream returned by the concrete implementation.</typeparam>
	public abstract class AbstractSource<T> : IFilesSource where T : Stream
	{
		#region Fields and Properties

		private string _name;
		/// <summary>
		/// Gets or sets the name of the source.
		/// </summary>
		/// <value>The name.</value>
		public string Name
		{
			get { return _name; }
			protected internal set { _name = value; }
		}

        /// <summary>
        /// Gets a value indicating whether this file source is read only.
        /// </summary>
        /// <value>
        /// <c>true</c> if this file source is read only; otherwise, <c>false</c>.
        /// </value>
	    public virtual bool IsReadOnly
	    {
            get { return true; }
	    }

	    private readonly CaseCultureStringComparer _stringComparer;
		/// <summary>
		/// Gets or sets the string comparer helper.
		/// </summary>
		/// <value>The string comparer.</value>
		protected CaseCultureStringComparer StringComparer
		{
			get { return _stringComparer; }
		}

		/// <summary>
		/// Gets or sets a value indicating whether this instance is case sensitive.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is case sensitive; otherwise, <c>false</c>.
		/// </value>
		public bool IsCaseSensitive
		{
			get { return _stringComparer.IsCaseSensitive; }
			set { _stringComparer.IsCaseSensitive = value; }
		}

		/// <summary>
		/// Gets or sets a value indicating whether this instance uses the invariant culture.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance uses the invariant culture; otherwise, <c>false</c>.
		/// </value>
		public bool IsInvariantCulture
		{
			get { return _stringComparer.IsInvariantCulture; }
			set { _stringComparer.IsInvariantCulture = value; }
		}

		/// <summary>
		/// Gets the current culture.
		/// </summary>
		/// <value>The current culture.</value>
		public CultureInfo CultureInfo
		{
			get { return _stringComparer.CultureInfo; }
		}

		private readonly Dictionary<string, string> _definedPaths;
		/// <summary>
		/// Gets the defined paths inside the source.
		/// </summary>
		/// <value>The defined paths.</value>
		protected Dictionary<string, string> DefinedPaths
		{
			get { return _definedPaths; }
		}

		private int _sleepTime = 10;
		/// <summary>
		/// Gets or sets the sleep time when a request is blocked.
		/// </summary>
		/// <value>The sleep time in milliseconds.</value>
		protected internal int SleepTime
		{
			get { return _sleepTime; }
			set { _sleepTime = value; }
		}

		/// <summary>
		/// List of used files in a given moment.
		/// </summary>
		private readonly Dictionary<string, ICollection<StreamDecorator<T>>> _usedFiles;

		/// <summary>
		/// Object to perform synchronization.
		/// </summary>
		private readonly object _lockInstance = new object();

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="AbstractSource&lt;T&gt;"/> class.
		/// </summary>
		protected AbstractSource()
		{
			_stringComparer = new CaseCultureStringComparer(true, false, CultureInfo.CurrentCulture.Name);
			_definedPaths = new Dictionary<string, string>(_stringComparer);
			_usedFiles = new Dictionary<string, ICollection<StreamDecorator<T>>>(_stringComparer);
		}

		#endregion

		#region IDisposable Members

		private bool _disposed;

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				lock (_lockInstance)
				{
					_disposed = true;

					if (disposing)
					{
						foreach (var stream in _usedFiles.SelectMany(o => o.Value))
							stream.Closed -= StreamClosed;

                        foreach (var openCopyCollection in _usedFiles.Values)
                            openCopyCollection.Clear();

						_usedFiles.Clear();
					}
				}
			}
		}

        protected void LogOpenFile(StreamDecorator<T> file)
        {
            lock (_lockInstance)
            {
                CheckDisposed();

                string key = file.VirtualPath;
                ICollection<StreamDecorator<T>> openCopies;
                if (!_usedFiles.TryGetValue(key, out openCopies))
                {
                    openCopies = new List<StreamDecorator<T>>();
                    _usedFiles[key] = openCopies;
                } 
                openCopies.Add(file);
            }
        }

	    protected void CheckDisposed()
	    {
	        lock(_lockInstance)
	        {
                if (_disposed)
                    throw new ObjectDisposedException(Name ?? GetType().Name);
	        }
	    }

	    ~AbstractSource()
		{
			Dispose(false);
		}

		#endregion

		#region Methods

		//This method creates a stream from a file request
        protected Stream BaseGetStream(string path, bool recurse, FileAccess access, bool block)
        {
            return BaseGetStream(path, recurse, access, GetDefaultFileShareForAccess(access), block);
        }

		protected Stream BaseGetStream(string path, bool recurse, FileAccess access, FileShare fileShare, bool block)
		{
		    // Try get the file (will only succeed if no one is using the file)
			while (true)
			{
			    string resolvedName;
			    lock (_lockInstance)
				{
					if (_disposed) // Source is disposed
						return null;

					// Resolve the file name. If it's not found return
					resolvedName = ResolveFileName(path, recurse);
					if (_stringComparer.Equals(resolvedName, string.Empty))
						return null;

					Debug.WriteLine("Trying to get file {0}", resolvedName);

                    if (CanAccessFile(path, GetMinimumFileShareForAccess(access))) // File not used
					{
						Debug.WriteLine("Got file {0}", resolvedName);

					    // Source specific get operation
                        T stream = InternalGetFile(resolvedName, access, fileShare);

						// Wrap the stream in a decorator
                        var decorator = new StreamDecorator<T>(resolvedName, stream, access, fileShare)
					                    {
					                        VirtualPath = path,
					                        SourceName = _name
					                    };

					    // Callback for when the stream closes
						decorator.Closed += StreamClosed;

						LogOpenFile(decorator);
						return decorator;
					}
				}

				// If the search should block then sleep
				if (block)
				{
					Debug.WriteLine("Blocked trying to get file {0}", resolvedName);
					Thread.Sleep(_sleepTime);
                    continue;
				}

			    return null;
			}
		}

	    private static FileShare GetDefaultFileShareForAccess(FileAccess access)
	    {
	        switch (access)
	        {
	            case FileAccess.Read:
	                return FileShare.Read;
	            case FileAccess.Write:
	                return FileShare.None;
	            case FileAccess.ReadWrite:
	                return FileShare.None;
	            default:
	                throw new ArgumentOutOfRangeException("fileAccess");
	        }
	    }

	    protected static FileShare GetMinimumFileShareForAccess(FileAccess access)
	    {
	        switch (access)
	        {
	            case FileAccess.Read:
	                return FileShare.Read;
	            case FileAccess.Write:
                    return FileShare.Write;
	            case FileAccess.ReadWrite:
                    return FileShare.ReadWrite;
	        }
	        return FileShare.None;
	    }

	    protected virtual bool CanAccessFile(string path, FileShare minimumShareLevel)
	    {
            lock (_lockInstance)
            {
                ICollection<StreamDecorator<T>> openCopies;
                if (!_usedFiles.TryGetValue(path, out openCopies))
                    return true;

                switch (minimumShareLevel)
                {
                    case FileShare.Delete:
                        return false;
                    default:
                        return !openCopies.Any(o => ((o.Share & minimumShareLevel) != minimumShareLevel));
                }
            }
	    }

	    //This method creates a new writable stream
		protected Stream BaseCreateStream(string path, bool block)
		{
		    bool canAccessFile = false;

			// Try get the file (will only succeed if no one is using the file)
			while (!canAccessFile)
			{
				lock (_lockInstance)
				{
					if (_disposed) // Source is disposed
						return null;

					// Resolve the file name. If found the file exists
					string resolvedName = ResolveFileName(path, false);
					bool fileExists = !_stringComparer.Equals(resolvedName, string.Empty);

					T stream = null;
					if (!fileExists) // Create the file
					{
					    stream = InternalCreateFile(resolvedName);
					}
					else
					{
						canAccessFile = CanAccessFile(path, GetMinimumFileShareForAccess(FileAccess.Write));
						if (canAccessFile) // File not used
                            stream = InternalGetFile(resolvedName, FileAccess.ReadWrite, FileShare.None);
					}

					if (stream != null)
					{
						// Wrap the stream in a decorator
                        var decorator = new StreamDecorator<T>(resolvedName, stream, FileAccess.ReadWrite, FileShare.None)
					                    {
					                        VirtualPath = path,
					                        SourceName = _name
					                    };

					    // Callback for when the stream closes
						decorator.Closed += StreamClosed;

						LogOpenFile(decorator);
						return decorator;
					}
				}

				// If the search should block then sleep
				if (block)
				{
				    Thread.Sleep(_sleepTime);
                    continue;
				}

			    return null;
			}

			return null;
		}

		//This method removes a file from the source
		protected bool BaseDeleteStream(string path, bool recurse, bool block)
		{
		    // Try get the file (will only succeed if no one is using the file)
			while (true)
			{
				lock (_lockInstance)
				{
					if (_disposed) // Source is disposed
						return false;

					// Resolve the file name. If it's not found the file doesn't exist
					string resolvedName = ResolveFileName(path, recurse);
					if (_stringComparer.Equals(resolvedName, string.Empty))
						return false;

					if (CanAccessFile(path, FileShare.Delete)) // File not used
					{
						// Source specific delete operation
						InternalDeleteFile(resolvedName);
						return true;
					}
				}

				// If the search should block then sleep
				if (block)
				{
				    Thread.Sleep(_sleepTime);
                    continue;
				}

			    return false;
			}
		}

		#endregion

		#region Virtual Methods

		//This method resolves a real source file name from a request path
		protected abstract string ResolveFileName(string path, bool recurse);

		//Gets the stream from the resolved name
		protected virtual T InternalGetFile(string resolvedName, FileAccess access, FileShare share)
		{
			throw new NotSupportedException("This method must be overriden in child classes");
		}

		//Creates a file for the file source
		protected virtual T InternalCreateFile(string resolvedName)
		{
			throw new NotSupportedException("This method must be overriden in child classes");
		}

		//Deletes a file from the file source
		protected virtual void InternalDeleteFile(string resolvedName)
		{
			throw new NotSupportedException("This method must be overriden in child classes");
		}

		#endregion

		#region Helper Methods

	    private static readonly Lazy<Regex> PathAliasRegex = 
            new Lazy<Regex>(() => new Regex("^[_A-Za-z][_A-Za-z0-9]*^", RegexOptions.Compiled));

        protected bool CheckPathAlias(string pathAlias)
        {
            if (string.IsNullOrWhiteSpace(pathAlias))
                return false;

            return PathAliasRegex.Value.IsMatch(pathAlias);
        }

		//General code for when a stream is closed
		private void StreamClosed(object sender, EventArgs e)
		{
			var stream = (StreamDecorator<T>) sender;

			lock (_lockInstance)
			{
                string key = stream.VirtualPath;
			    ICollection<StreamDecorator<T>> openCopies;
			    
			    if (!_usedFiles.TryGetValue(key, out openCopies))
                    return;

				openCopies.Remove(stream);
				stream.Closed -= StreamClosed;

                if (openCopies.Count == 0)
                    _usedFiles.Remove(key);

				Debug.WriteLine("Stream {0} closed", stream.ResolvedPath);
			}
		}

		#endregion

		#region IFilesSource Members

		public void SetCultureName(string cultureName)
		{
			_stringComparer.SetCultureName(cultureName);
		}

		public virtual void AddDefinedPath(string definedPathAlias, string path)
		{
            if (!CheckPathAlias(definedPathAlias))
            {
                throw new ArgumentException(
                    string.Format(
                        "Invalid path alias name: '{0}'",
                        definedPathAlias),
                    "definedPathAlias");
            }

			_definedPaths.Add(definedPathAlias, path);
		}

	    public virtual IVirtualFileInfo GetFileInfo(string path, bool recurse)
	    {
	        throw new NotSupportedException();
	    }

        public virtual IVirtualFileInfo GetFileInfo(string definedPathAlias, string path, bool recurse)
        {
            if (!_definedPaths.TryGetValue(definedPathAlias, out string aliasedPath))
                return null;
            return GetFileInfo(path, recurse);
        }

	    public virtual Stream GetFile(string path, bool recurse)
		{
			return BaseGetStream(path, recurse, FileAccess.Read, true);
		}

		public Stream GetFile(string definedPathAlias, string path, bool recurse)
		{
            string aliasedPath;

			// If the path is not defined in this source, return
			if (!_definedPaths.TryGetValue(definedPathAlias, out aliasedPath))
				return null;

			return GetFile(Path.Combine(aliasedPath, path), recurse);
		}

		public virtual bool TryGetFile(string path, bool recurse, out Stream stream)
		{
			stream = BaseGetStream(path, recurse, FileAccess.Read, false);

			return stream != null;
		}

		public bool TryGetFile(string definedPathAlias, string path, bool recurse, out Stream stream)
		{
			String dir;

			// If the path is not defined in this source, return
			if (!_definedPaths.TryGetValue(definedPathAlias, out dir))
			{
				stream = null;
				return false;
			}

			return TryGetFile(Path.Combine(dir, path), recurse, out stream);
		}

		public virtual ReadOnlyCollection<string> GetFiles(string path, bool recurse, string searchPattern)
		{
			throw new NotImplementedException("The method or operation is not implemented.");
		}

		public ReadOnlyCollection<string> GetFiles(string definedPathAlias, string path, bool recurse, string searchPattern)
		{
			String dir;

			// If the path is not defined in this source, return
			if (!_definedPaths.TryGetValue(definedPathAlias, out dir))
				return new List<string>(0).AsReadOnly();

			return GetFiles(Path.Combine(dir, path), recurse, searchPattern);
		}

		#endregion
	}
}
