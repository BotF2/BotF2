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

using System;
using System.Collections.ObjectModel;
using System.IO;

namespace Supremacy.VFS
{
    /// <summary>
    /// Represents a place where files are located. The <see cref="IVfsService"/> 
    /// uses this objects to locate files.
    /// </summary>
    /// <example>
    /// Examples of file sources are:
    /// - hard disk paths (all files and directories in c:\mytextures).
    /// - storage files (files and directories inside the storage "c:\resources\mystorage.jsf").
    /// - ...
    /// </example>
    /// <remarks>
    /// Note to implementers: all methods from this class should allow only read access to the streams they return.
    /// To allow write access implement <see cref="IWritableFilesSource"/> instead.
    /// </remarks>
    public interface IFilesSource : IDisposable
    {
        #region Properties
        /// <summary>
        /// Gets the name of the source.
        /// </summary>
        /// <value>The name of the source.</value>
        string Name { get; }

        /// <summary>
        /// Gets a value indicating whether this file source is read only.
        /// </summary>
        /// <value>
        /// <c>true</c> if this file source is read only; otherwise, <c>false</c>.
        /// </value>
        bool IsReadOnly { get; }
        #endregion

        #region Methods
        /// <summary>
        /// Adds a new defined path to the <see cref="IFilesSource"/>.
        /// </summary>
        /// <param name="definedPathAlias">Name of the defined path.</param>
        /// <param name="path">Real path of the defined path.</param>
        void AddDefinedPath(string definedPathAlias, string path);

        /// <summary>
        /// Gets the metadata for the specified virtual file.
        /// </summary>
        /// <param name="path">The virtual path of the file.</param>
        /// <param name="recurse">
        /// If set to <c>true</c>, the parent directory specified in the virtual path and all subdirectories will be searched
        /// recursively until a file with a matching name is found.</param>
        /// <returns>The metadata for the specified virtual file.</returns>
        IVirtualFileInfo GetFileInfo(string path, bool recurse);

        /// <summary>
        /// Gets the metadata for the specified virtual file.
        /// </summary>
        /// <param name="definedPathAlias">The name of a defined path.</param>
        /// <param name="path">The virtual path of the file.</param>
        /// <param name="recurse">
        /// If set to <c>true</c>, the parent directory specified in the virtual path and all subdirectories will be searched
        /// recursively until a file with a matching name is found.</param>
        /// <returns>The metadata for the specified virtual file.</returns>
        IVirtualFileInfo GetFileInfo(string definedPathAlias, string path, bool recurse);

        /// <summary>
        /// Gets a stream to a file.
        /// </summary>
        /// <param name="path">ResolvedPath to the file.</param>
        /// <param name="recurse"><c>true</c> if the search should be recursive, <c>false</c> if not.</param>
        /// <returns>A stream to the file.</returns>
        Stream GetFile(string path, bool recurse);

        /// <summary>
        /// Gets a stream to a file.
        /// </summary>
        /// <param name="definedPathAlias">A defined path where to search the file.</param>
        /// <param name="path">ResolvedPath to the file.</param>
        /// <param name="recurse"><c>true</c> if the search should be recursive, <c>false</c> if not.</param>
        /// <returns>A stream to the file.</returns>
        Stream GetFile(string definedPathAlias, string path, bool recurse);

        /// <summary>
        /// Gets the collection of files on a directory.
        /// </summary>
        /// <param name="path">ResolvedPath of the directory.</param>
        /// <param name="recurse"><c>true</c> if the search should be recursive, <c>false</c> if not.</param>
        /// <param name="searchPattern">Mask to filter the files.</param>
        /// <returns>The collection of files of the directory.</returns>
        ReadOnlyCollection<string> GetFiles(string path, bool recurse, string searchPattern);

        /// <summary>
        /// Gets the collection of files on a path from a defined path.
        /// </summary>
        /// <param name="definedPathAlias">Base path for the search.</param>
        /// <param name="path">ResolvedPath inside the defined path.</param>
        /// <param name="recurse"><c>true</c> if the search should be recursive, <c>false</c> if not.</param>
        /// <param name="searchPattern">Mask to filter the files.</param>
        /// <returns>The collection of files of the directory.</returns>
        ReadOnlyCollection<string> GetFiles(string definedPathAlias, string path, bool recurse, string searchPattern);

        /// <summary>
        /// Sets the culture.
        /// </summary>
        /// <param name="cultureName">Name of the culture.</param>
        void SetCultureName(string cultureName);

        /// <summary>
        /// Tries to get a stream to a file. If the stream is locked by another thread it returns without
        /// blocking.
        /// </summary>
        /// <param name="path">ResolvedPath to the file.</param>
        /// <param name="recurse"><c>true</c> if the search should be recursive, <c>false</c> if not.</param>
        /// <param name="stream">The stream if the method could get the lock to it. Null if not.</param>
        /// <returns><c>true</c> if the method could get the stream, <c>false</c> if not.</returns>
        bool TryGetFile(string path, bool recurse, out Stream stream);

        /// <summary>
        /// Tries to get a stream to a file. If the stream is locked by another thread it returns without
        /// blocking.
        /// </summary>
        /// <param name="definedPathAlias">A defined path where to search the file.</param>
        /// <param name="path">ResolvedPath to the file.</param>
        /// <param name="recurse"><c>true</c> if the search should be recursive, <c>false</c> if not.</param>
        /// <param name="stream">The stream if the method could get the lock to it. Null if not.</param>
        /// <returns><c>true</c> if the method could get the stream, <c>false</c> if not.</returns>
        bool TryGetFile(string definedPathAlias, string path, bool recurse, out Stream stream);
        #endregion
    }
}