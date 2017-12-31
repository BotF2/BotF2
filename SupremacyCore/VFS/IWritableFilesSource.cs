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

using System.IO;

namespace Supremacy.VFS
{
    /// <summary>
    /// A <see cref="IWritableFilesSource"/> is a special type of <see cref="IFilesSource"/> that
    /// allows write operations on files, or to create files inside the source.
    /// </summary>
    public interface IWritableFilesSource : IFilesSource
    {
        #region Methods
        /// <summary>
        /// Creates a new file.
        /// </summary>
        /// <param name="path">ResolvedPath to the new file.</param>
        /// <returns>A stream to the new file.</returns>
        Stream CreateFile(string path);

        /// <summary>
        /// Creates a new file.
        /// </summary>
        /// <param name="definedPathAlias">Defined path where the file will be located.</param>
        /// <param name="path">ResolvedPath to the new file.</param>
        /// <returns>A stream to the new file.</returns>
        Stream CreateFile(string definedPathAlias, string path);

        /// <summary>
        /// Gets a read/write stream to a file.
        /// </summary>
        /// <param name="path">ResolvedPath to the file.</param>
        /// <param name="recurse"><c>true</c> if the search should be recursive, <c>false</c> if not.</param>
        /// <returns>A stream to the file.</returns>
        Stream GetWritableFile(string path, bool recurse);

        /// <summary>
        /// Gets a read/write to a file.
        /// </summary>
        /// <param name="definedPathAlias">A defined path where to search the file.</param>
        /// <param name="path">ResolvedPath to the file.</param>
        /// <param name="recurse"><c>true</c> if the search should be recursive, <c>false</c> if not.</param>
        /// <returns>A stream to the file.</returns>
        Stream GetWritableFile(string definedPathAlias, string path, bool recurse);

        /// <summary>
        /// Removes a file.
        /// </summary>
        /// <param name="path">ResolvedPath to the file.</param>
        /// <param name="recurse"><c>true</c> if the search should be recursive, <c>false</c> if not.</param>
        /// <returns></returns>
        bool RemoveFile(string path, bool recurse);

        /// <summary>
        /// Removes a file.
        /// </summary>
        /// <param name="definedPathAlias">A defined path where to search the file.</param>
        /// <param name="path">ResolvedPath to the file.</param>
        /// <param name="recurse"><c>true</c> if the search should be recursive, <c>false</c> if not.</param>
        /// <returns><c>true</c> if the file was removed, <c>false</c> if not.</returns>
        bool RemoveFile(string definedPathAlias, string path, bool recurse);

        /// <summary>
        /// Tries to creates a new file.
        /// </summary>
        /// <param name="path">ResolvedPath to the new file.</param>
        /// <param name="stream">The stream if the method could create or get it. Null if not.</param>
        /// <returns><c>true</c> if the method could create the stream, <c>false</c> if not.</returns>
        bool TryCreateFile(string path, out Stream stream);

        /// <summary>
        /// Tries to creates a new file.
        /// </summary>
        /// <param name="definedPathAlias">Defined path where the file will be located.</param>
        /// <param name="path">ResolvedPath to the new file.</param>
        /// <param name="stream">The stream if the method could create or get it. Null if not.</param>
        /// <returns><c>true</c> if the method could create the stream, <c>false</c> if not.</returns>
        bool TryCreateFile(string definedPathAlias, string path, out Stream stream);

        /// <summary>
        /// Tries to get a read/write stream to a file. If the stream is locked by
        /// another thread it returns without blocking.
        /// </summary>
        /// <param name="path">ResolvedPath to the file.</param>
        /// <param name="recurse"><c>true</c> if the search should be recursive, <c>false</c> if not.</param>
        /// <param name="stream">The stream if the method could get the lock to it. Null if not.</param>
        /// <returns><c>true</c> if the method could get the stream, <c>false</c> if not.</returns>
        bool TryGetWritableFile(string path, bool recurse, out Stream stream);

        /// <summary>
        /// Tries to get a read/write stream to a file. If the stream is locked by
        /// another thread it returns without blocking.
        /// </summary>
        /// <param name="definedPathAlias">A defined path where to search the file.</param>
        /// <param name="path">ResolvedPath to the file.</param>
        /// <param name="recurse"><c>true</c> if the search should be recursive, <c>false</c> if not.</param>
        /// <param name="stream">The stream if the method could get the lock to it. Null if not.</param>
        /// <returns><c>true</c> if the method could get the stream, <c>false</c> if not.</returns>
        bool TryGetWritableFile(string definedPathAlias, string path, bool recurse, out Stream stream);

        /// <summary>
        /// Tries to remove a file. If the stream is locked by another thread it returns 
        /// without blocking.
        /// </summary>
        /// <param name="path">ResolvedPath to the file.</param>
        /// <param name="recurse"><c>true</c> if the search should be recursive, <c>false</c> if not.</param>
        /// <returns></returns>
        bool TryRemoveFile(string path, bool recurse);

        /// <summary>
        /// Tries to remove a file. If the stream is locked by another thread it returns 
        /// without blocking.
        /// </summary>
        /// <param name="definedPathAlias">A defined path where to search the file.</param>
        /// <param name="path">ResolvedPath to the file.</param>
        /// <param name="recurse"><c>true</c> if the search should be recursive, <c>false</c> if not.</param>
        /// <returns><c>true</c> if the file was removed, <c>false</c> if not.</returns>
        bool TryRemoveFile(string definedPathAlias, string path, bool recurse);
        #endregion
    }
}