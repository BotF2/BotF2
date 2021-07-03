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

using System.IO;

#endregion

namespace Supremacy.VFS
{
    /// <summary>
    /// Abstract implementation of an <see cref="IWritableFilesSource"/>.
    /// </summary>
    /// <typeparam name="T">The type of Stream used internally.</typeparam>
	public abstract class AbstractWritableSource<T> : AbstractSource<T>, IWritableFilesSource where T : Stream
    {
        #region IWritableFilesSource Members
        /// <summary>
        /// Creates a new file.
        /// </summary>
        /// <param name="path">ResolvedPath to the new file.</param>
        /// <returns>A stream to the new file.</returns>
        public virtual Stream CreateFile(string path)
        {
            return BaseCreateStream(path, true);
        }

        /// <summary>
        /// Creates a new file.
        /// </summary>
        /// <param name="definedPathAlias">Defined path where the file will be located.</param>
        /// <param name="path">ResolvedPath to the new file.</param>
        /// <returns>A stream to the new file.</returns>
        public Stream CreateFile(string definedPathAlias, string path)
        {

            // If the path is not defined in this source, return
            if (!DefinedPaths.TryGetValue(definedPathAlias, out string dir))
            {
                return null;
            }

            return CreateFile(Path.Combine(dir, path));
        }

        /// <summary>
        /// Tries to creates a new file.
        /// </summary>
        /// <param name="path">ResolvedPath to the new file.</param>
        /// <param name="stream">The stream if the method could create or get it. Null if not.</param>
        /// <returns>
        /// 	<c>true</c> if the method could create the stream, <c>false</c> if not.
        /// </returns>
        public virtual bool TryCreateFile(string path, out Stream stream)
        {
            stream = BaseCreateStream(path, false);

            return stream != null;
        }

        /// <summary>
        /// Tries to creates a new file.
        /// </summary>
        /// <param name="definedPathAlias">Defined path where the file will be located.</param>
        /// <param name="path">ResolvedPath to the new file.</param>
        /// <param name="stream">The stream if the method could create or get it. Null if not.</param>
        /// <returns>
        /// 	<c>true</c> if the method could create the stream, <c>false</c> if not.
        /// </returns>
        public bool TryCreateFile(string definedPathAlias, string path, out Stream stream)
        {

            // If the path is not defined in this source, return
            if (!DefinedPaths.TryGetValue(definedPathAlias, out string dir))
            {
                stream = null;
                return false;
            }

            return TryCreateFile(Path.Combine(dir, path), out stream);
        }

        /// <summary>
        /// Gets a read/write stream to a file.
        /// </summary>
        /// <param name="path">ResolvedPath to the file.</param>
        /// <param name="recurse"><c>true</c> if the search should be recursive, <c>false</c> if not.</param>
        /// <returns>A stream to the file.</returns>
        public virtual Stream GetWritableFile(string path, bool recurse)
        {
            return BaseGetStream(path, recurse, FileAccess.ReadWrite, true);
        }

        /// <summary>
        /// Gets a read/write to a file.
        /// </summary>
        /// <param name="definedPathAlias">A defined path where to search the file.</param>
        /// <param name="path">ResolvedPath to the file.</param>
        /// <param name="recurse"><c>true</c> if the search should be recursive, <c>false</c> if not.</param>
        /// <returns>A stream to the file.</returns>
        public Stream GetWritableFile(string definedPathAlias, string path, bool recurse)
        {

            // If the path is not defined in this source, return
            if (!DefinedPaths.TryGetValue(definedPathAlias, out string dir))
            {
                return null;
            }

            return GetWritableFile(Path.Combine(dir, path), recurse);
        }

        /// <summary>
        /// Tries to get a read/write stream to a file. If the stream is locked by
        /// another thread it returns without blocking.
        /// </summary>
        /// <param name="path">ResolvedPath to the file.</param>
        /// <param name="recurse"><c>true</c> if the search should be recursive, <c>false</c> if not.</param>
        /// <param name="stream">The stream if the method could get the lock to it. Null if not.</param>
        /// <returns>
        /// 	<c>true</c> if the method could get the stream, <c>false</c> if not.
        /// </returns>
        public virtual bool TryGetWritableFile(string path, bool recurse, out Stream stream)
        {
            stream = BaseGetStream(path, recurse, FileAccess.ReadWrite, false);

            return stream != null;
        }

        /// <summary>
        /// Tries to get a read/write stream to a file. If the stream is locked by
        /// another thread it returns without blocking.
        /// </summary>
        /// <param name="definedPathAlias">A defined path where to search the file.</param>
        /// <param name="path">ResolvedPath to the file.</param>
        /// <param name="recurse"><c>true</c> if the search should be recursive, <c>false</c> if not.</param>
        /// <param name="stream">The stream if the method could get the lock to it. Null if not.</param>
        /// <returns>
        /// 	<c>true</c> if the method could get the stream, <c>false</c> if not.
        /// </returns>
        public bool TryGetWritableFile(string definedPathAlias, string path, bool recurse, out Stream stream)
        {

            // If the path is not defined in this source, return
            if (!DefinedPaths.TryGetValue(definedPathAlias, out string dir))
            {
                stream = null;
                return false;
            }

            return TryGetWritableFile(Path.Combine(dir, path), recurse, out stream);
        }

        /// <summary>
        /// Removes a file.
        /// </summary>
        /// <param name="path">ResolvedPath to the file.</param>
        /// <param name="recurse"><c>true</c> if the search should be recursive, <c>false</c> if not.</param>
        /// <returns></returns>
        public virtual bool RemoveFile(string path, bool recurse)
        {
            return BaseDeleteStream(path, recurse, true);
        }

        /// <summary>
        /// Removes a file.
        /// </summary>
        /// <param name="definedPathAlias">A defined path where to search the file.</param>
        /// <param name="path">ResolvedPath to the file.</param>
        /// <param name="recurse"><c>true</c> if the search should be recursive, <c>false</c> if not.</param>
        /// <returns>
        /// 	<c>true</c> if the file was removed, <c>false</c> if not.
        /// </returns>
        public bool RemoveFile(string definedPathAlias, string path, bool recurse)
        {

            // If the path is not defined in this source, return
            if (!DefinedPaths.TryGetValue(definedPathAlias, out string dir))
            {
                return false;
            }

            return RemoveFile(Path.Combine(dir, path), recurse);
        }

        /// <summary>
        /// Tries to remove a file. If the stream is locked by another thread it returns
        /// without blocking.
        /// </summary>
        /// <param name="path">ResolvedPath to the file.</param>
        /// <param name="recurse"><c>true</c> if the search should be recursive, <c>false</c> if not.</param>
        /// <returns></returns>
        public virtual bool TryRemoveFile(string path, bool recurse)
        {
            return BaseDeleteStream(path, recurse, false);
        }

        /// <summary>
        /// Tries to remove a file. If the stream is locked by another thread it returns
        /// without blocking.
        /// </summary>
        /// <param name="definedPathAlias">A defined path where to search the file.</param>
        /// <param name="path">ResolvedPath to the file.</param>
        /// <param name="recurse"><c>true</c> if the search should be recursive, <c>false</c> if not.</param>
        /// <returns>
        /// 	<c>true</c> if the file was removed, <c>false</c> if not.
        /// </returns>
        public bool TryRemoveFile(string definedPathAlias, string path, bool recurse)
        {

            // If the path is not defined in this source, return
            if (!DefinedPaths.TryGetValue(definedPathAlias, out string dir))
            {
                return false;
            }

            return TryRemoveFile(Path.Combine(dir, path), recurse);
        }
        #endregion
    }
}
