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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;

using IOPath = System.IO.Path;

#endregion

namespace Supremacy.VFS
{
	/// <summary>
	/// Type of compression used.
	/// </summary>
	public enum CompressionAlgorithm
	{
		/// <summary>
		/// No compression used.
		/// </summary>
		None,
		/// <summary>
		/// GZip compression used.
		/// </summary>
		GZip,
		/// <summary>
		/// Deflate compression used.
		/// </summary>
		Deflate
	}

	///<summary>
	/// An in-memory file source.
	///</summary>
	public class MemorySource : AbstractWritableSource<MemoryFileStream>
	{
		#region Fields and Properties

		private int _defaultFilesSize;
		/// <summary>
		/// Gets or sets the default size of the files (in KB).
		/// </summary>
		/// <value>The size the default size of the files (in KB).</value>
		public int DefaultFilesSize
		{
			get { return _defaultFilesSize; }
			set { _defaultFilesSize = value; }
		}

		/// <summary>
		/// Memory buffers to hold the files of the source.
		/// </summary>
		private readonly Dictionary<string, byte[]> _files;

		private CompressionAlgorithm _compressionAlgorithm = CompressionAlgorithm.None;
		/// <summary>
		/// Gets or sets the compression mode.
		/// </summary>
		/// <value>The compression mode.</value>
		public CompressionAlgorithm CompressionAlgorithm
		{
			get { return _compressionAlgorithm; }
			set { _compressionAlgorithm = value; }
		}

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="MemorySource"/> class.
		/// </summary>
		/// <param name="name">The name of the <see cref="MemorySource"/>.</param>
		public MemorySource(string name)
		{
			Name = name;
			_defaultFilesSize = 1024;
			_files = new Dictionary<string, byte[]>(StringComparer);
		}

		#endregion

		#region Abstract Source Methods

		protected override string ResolveFileName(string path, bool recurse)
		{
			if (!recurse)
			{
				if (_files.ContainsKey(path))
					return path;
			}
			else
			{
			    string dir = IOPath.GetDirectoryName(path);
				string name = IOPath.GetFileName(path);

                foreach (string key in _files.Keys)
                {
                    if (StringComparer.Equals(Path.GetDirectoryName(key), dir) &&
                        StringComparer.Equals(Path.GetFileName(key), name))
                    {
                        return key;
                    }
                }
			}

			return string.Empty;
		}

		protected override MemoryFileStream InternalGetFile(string resolvedName, FileAccess access, FileShare share)
		{
			byte[] fileData = _files[resolvedName];
			MemoryStream stream;

			if (access == FileAccess.Read)
			{
			    stream = new MemoryStream(fileData);
			}
			else
			{
				stream = new MemoryStream(fileData.Length);
				
				int bytesCopied;
				for (bytesCopied = 0; bytesCopied < fileData.Length; bytesCopied += 1024)
				{
				    stream.Write(fileData, bytesCopied, 1024);
				}

				stream.Write(fileData, bytesCopied, fileData.Length - bytesCopied);
			}

            MemoryFileStream memoryStream = new MemoryFileStream(this, resolvedName, stream, access, share);

			if (access == FileAccess.Read)
			{
			    memoryStream.SetCompression(_compressionAlgorithm, CompressionMode.Decompress);
			}
			else
			{
			    memoryStream.SetCompression(_compressionAlgorithm, CompressionMode.Compress);
			}

			return memoryStream;
		}

		protected override MemoryFileStream InternalCreateFile(string resolvedName)
		{
			_files.Add(resolvedName, null);

            MemoryStream stream = new MemoryStream(_defaultFilesSize * 1024);
            MemoryFileStream memoryStream = new MemoryFileStream(this, resolvedName, stream, FileAccess.ReadWrite, FileShare.None);

			memoryStream.SetCompression(_compressionAlgorithm, CompressionMode.Compress);

			return memoryStream;
		}

		protected override void InternalDeleteFile(string resolvedName)
		{
			_files.Remove(resolvedName);
		}

		#endregion

		#region IFilesSource Methods

		public override ReadOnlyCollection<string> GetFiles(string path, bool recurse, string searchPattern)
		{
            List<string> results = new List<string>();

            foreach (string key in _files.Keys)
            {
                if (key.StartsWith(path, !IsCaseSensitive, CultureInfo) &&
                    Utility.Match(key, searchPattern, IsCaseSensitive, CultureInfo))
                {
                    results.Add(key);
                }
            }

		    return results.AsReadOnly();
		}

		#endregion

		#region Methods

		internal void UpdateFileBuffer(MemoryFileStream stream)
		{
			_files[stream.ResolvedPath] = stream.GetBuffer();
		}

		#endregion
	}
}
