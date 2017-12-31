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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

using Supremacy.Collections;
using Supremacy.VFS.Utilities;

namespace Supremacy.VFS
{
    /// <summary>
    /// Represents a virtual file system.
    /// </summary>
    /// <remarks>
    /// The priority in the searches within this object is related to the order in which the
    /// <see cref="IFilesSource"/> items are aggregated. The first item gets higher priority
    /// and so on.
    /// </remarks>
    public class VfsService : IVfsService
    {
        #region Fields

        public const string UriScheme = "vfs";

        private string _name = "Default";

        /// <summary>
        /// The list of registered <see cref="IFilesSource"/> of the <see cref="VfsService"/>.
        /// </summary>
        /// <remarks>
        /// These objects are the ones that have the real references to the files,
        /// the <see cref="VfsService"/> only routes the petitions to them.
        /// </remarks>
        private readonly KeyedCollectionBase<string, IFilesSource> _sources = new KeyedCollectionBase<string, IFilesSource>(o => o.Name, System.StringComparer.OrdinalIgnoreCase);

        public CaseCultureStringComparer StringComparer { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="VfsService"/> class.
        /// </summary>
        public VfsService()
        {
            StringComparer = new CaseCultureStringComparer(false, true, null);
        }

        public IIndexedCollection<IFilesSource> Sources
        {
            get { return _sources; }
        }

        /// <summary>
        /// Dispose pattern.
        /// </summary>
        private bool _disposed;
        #endregion

        #region Methods
        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            var exceptions = new List<Exception>(_sources.Count);

            if (disposing)
            {
                foreach (var source in _sources)
                {
                    try
                    {
                        source.Dispose();
                    }
                    catch (Exception e)
                    {
                        exceptions.Add(e);
                    }
                }
            }

            _disposed = true;

            if (exceptions.Count != 0)
                throw new AggregateException(exceptions);
        }

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="VfsService"/> is reclaimed by garbage collection.
        /// </summary>
        ~VfsService()
        {
            Dispose(false);
        }
        #endregion

        #region IVFSService Members
        /// <summary>
        /// Gets the name of the <see cref="VfsService"/>.
        /// </summary>
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public virtual bool IsReadOnly
        {
            get { return !_sources.Any(source => !source.IsReadOnly); }
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Gets or sets the file resolution mode used by the <see cref="IVfsService"/>.
        /// </summary>
        /// <value>The file resolution mode.</value>
        public VfsFileResolutionMode FileResolutionMode { get; set; }

        /// <summary>
        /// Adds a new <see cref="IFilesSource"/> to the <see cref="IVfsService"/>.
        /// </summary>
        /// <param name="source"><see cref="IFilesSource"/> to add.</param>
        public void AddSource(IFilesSource source)
        {
            _sources.Add(source);
        }

        /// <summary>
        /// Gets a <see cref="IFilesSource"/>.
        /// </summary>
        /// <param name="sourceName">Name of the <see cref="IFilesSource"/> to get.</param>
        /// <returns>
        /// The selected <see cref="IFilesSource"/>.
        /// </returns>
        public IFilesSource GetSource(string sourceName)
        {
            IFilesSource source;
            
            if (_sources.TryGetValue(sourceName, out source))
                return source;

            throw new IOException("The source \"" + sourceName + "\" doesn't exist in the VFS \"" + _name + "\".");
        }

        /// <summary>
        /// Gets a <see cref="IWritableFilesSource"/>.
        /// </summary>
        /// <param name="sourceName">Name of the <see cref="IWritableFilesSource"/> to get.</param>
        /// <returns>
        /// The selected <see cref="IWritableFilesSource"/>.
        /// </returns>
        public IWritableFilesSource GetWritableSource(string sourceName)
        {
            foreach (var source in _sources)
            {
                if (!string.Equals(source.Name, sourceName, StringComparison.Ordinal))
                    continue;
                
                var writableSource = source as IWritableFilesSource;
                if (writableSource != null)
                    return writableSource;

                throw new IOException(
                    "The source \"" + sourceName + "\" is not a IWritableFilesSource in the VFS \"" + _name + "\".");
            }

            throw new IOException("The source \"" + sourceName + "\" doesn't exist in the VFS \"" + _name + "\".");
        }

        public IVirtualFileInfo GetFile(string virtualPath)
        {
            return GetFileInfo(virtualPath, false);
        }

        public IVirtualFileInfo GetWriteableFile(string virtualPath)
        {
            return _sources
                .Where(o => !o.IsReadOnly)
                .Select(o => o.GetFileInfo(virtualPath))
                .FirstOrDefault();
        }

        ///// <summary>
        ///// Sets the culture.
        ///// </summary>
        ///// <param name="cultureName">Name of the culture.</param>
        //void IFilesSource.SetCultureName(string cultureName)
        //{
        //    throw new InvalidOperationException(
        //        "Operation not supported directly on VFSService; use directly on sources instead.");
        //}

        ///// <summary>
        ///// Adds a new defined path to the <see cref="IFilesSource"/>.
        ///// </summary>
        ///// <param name="definedPathAlias">Name of the defined path.</param>
        ///// <param name="path">Real path of the defined path.</param>
        ///// <remarks>Does nothing.</remarks>
        //void IFilesSource.AddDefinedPath(string definedPathAlias, string path)
        //{
        //    throw new InvalidOperationException(
        //        "Operation not supported directly on VFSService; use directly on sources instead.");
        //}

        /// <summary>
        /// Gets the metadata for the specified virtual file.
        /// </summary>
        /// <param name="path">The virtual path of the file.</param>
        /// <param name="recurse">If set to <c>true</c>, the parent directory specified in the virtual path and all subdirectories will be searched
        /// recursively until a file with a matching name is found.</param>
        /// <returns>
        /// The metadata for the specified virtual file.
        /// </returns>
        public IVirtualFileInfo GetFileInfo(string path, bool recurse)
        {
            var fileInfos = _sources
                .Select(o => o.GetFileInfo(path, recurse))
                .Where(o => (o != null));
            
            switch (FileResolutionMode)
            {
                case VfsFileResolutionMode.ReturnNewestMatch:
                    fileInfos = fileInfos.OrderByDescending(o => o.LastWriteTimeUtc);
                    break;
            }

            return fileInfos.Where(o => o.Exists).FirstOrDefault()
                   ?? fileInfos.FirstOrDefault();
        }

        public IVirtualFileInfo GetFileInfo(string definedPathAlias, string path, bool recurse)
        {
            var fileInfos = _sources
                .Select(o => o.GetFileInfo(definedPathAlias, path, recurse))
                .Where(o => (o != null));

            switch (FileResolutionMode)
            {
                case VfsFileResolutionMode.ReturnNewestMatch:
                    fileInfos = fileInfos.OrderByDescending(o => o.LastWriteTimeUtc);
                    break;
            }

            return fileInfos.Where(o => o.Exists).FirstOrDefault()
                   ?? fileInfos.FirstOrDefault();
        }

        /// <summary>
        /// Gets a stream to a file.
        /// </summary>
        /// <param name="path">ResolvedPath to the file.</param>
        /// <param name="recurse">If the search should be recursive or not.</param>
        /// <returns>A stream to the file.</returns>
        public Stream GetFile(string path, bool recurse)
        {
            var fileInfos = _sources
                .Select(o => o.GetFileInfo(path, recurse))
                .Where(o => (o != null) && o.Exists);
            
            switch (FileResolutionMode)
            {
                case VfsFileResolutionMode.ReturnNewestMatch:
                    fileInfos = fileInfos.OrderByDescending(o => o.LastWriteTimeUtc);
                    break;
            }

            return fileInfos.FirstOrDefault().OpenRead();
        }

        /// <summary>
        /// Gets a stream to a file.
        /// </summary>
        /// <param name="definedPathAlias">A defined path where to search the file.</param>
        /// <param name="path">ResolvedPath to the file.</param>
        /// <param name="recurse">If the search should be recursive or not.</param>
        /// <returns>A stream to the file.</returns>
        public Stream GetFile(string definedPathAlias, string path, bool recurse)
        {
            var fileInfos = _sources
                            .Select(o => o.GetFileInfo(definedPathAlias, path, recurse))
                            .Where(o => (o != null) && o.Exists);

            switch (FileResolutionMode)
            {
                case VfsFileResolutionMode.ReturnNewestMatch:
                    fileInfos = fileInfos.OrderByDescending(o => o.LastWriteTimeUtc);
                    break;
            }

            return fileInfos.FirstOrDefault().OpenRead();
        }

        /// <summary>
        /// Tries to get a stream to a file. If the stream is locked by another thread it returns without
        /// blocking.
        /// </summary>
        /// <param name="path">ResolvedPath to the file.</param>
        /// <param name="recurse"><c>true</c> if the search should be recursive, <c>false</c> if not.</param>
        /// <param name="stream">The stream if the method could get the lock to it. Null if not.</param>
        /// <returns>
        /// 	<c>true</c> if the method could get the stream, <c>false</c> if not.
        /// </returns>
        public bool TryGetFile(string path, bool recurse, out Stream stream)
        {
            try
            {
                stream = GetFile(path, recurse);
                return true;
            }
            catch
            {
                stream = null;
                return false;
            }
        }

        /// <summary>
        /// Tries to get a stream to a file. If the stream is locked by another thread it returns without
        /// blocking.
        /// </summary>
        /// <param name="definedPathAlias">A defined path where to search the file.</param>
        /// <param name="path">ResolvedPath to the file.</param>
        /// <param name="recurse"><c>true</c> if the search should be recursive, <c>false</c> if not.</param>
        /// <param name="stream">The stream if the method could get the lock to it. Null if not.</param>
        /// <returns>
        /// 	<c>true</c> if the method could get the stream, <c>false</c> if not.
        /// </returns>
        public bool TryGetFile(string definedPathAlias, string path, bool recurse, out Stream stream)
        {
            try
            {
                stream = GetFile(definedPathAlias, path, recurse);
                return true;
            }
            catch
            {
                stream = null;
                return false;
            }
        }

        /// <summary>
        /// Gets the collection of files on a directory.
        /// </summary>
        /// <param name="path">ResolvedPath of the directory.</param>
        /// <param name="recurse">If the search should be recursive or not.</param>
        /// <param name="searchPattern">Mask to filter the files.</param>
        /// <returns>
        /// The collection of files of the directory.
        /// </returns>
        public ReadOnlyCollection<string> GetFiles(string path, bool recurse, string searchPattern)
        {
            return _sources
                .SelectMany(o => o.GetFiles(path, recurse, searchPattern))
                .Where(o => o != null)
                .Distinct(System.StringComparer.OrdinalIgnoreCase)
                .ToList()
                .AsReadOnly();
        }

        /// <summary>
        /// Gets the collection of files on a path from a defined path.
        /// </summary>
        /// <param name="definedPathAlias">Base path for the search.</param>
        /// <param name="path">ResolvedPath inside the defined path.</param>
        /// <param name="recurse">If the search should be recursive or not.</param>
        /// <param name="searchPattern">Mask to filter the files.</param>
        /// <returns>
        /// The collection of files of the directory.
        /// </returns>
        public ReadOnlyCollection<string> GetFiles(string definedPathAlias, string path, bool recurse, string searchPattern)
        {
            return _sources
                .SelectMany(o => o.GetFiles(definedPathAlias, path, recurse, searchPattern))
                .Where(o => o != null)
                .Distinct(System.StringComparer.OrdinalIgnoreCase)
                .ToList()
                .AsReadOnly();
        }
        #endregion
    }
}