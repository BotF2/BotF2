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

using Supremacy.Collections;

namespace Supremacy.VFS
{
    /// <summary>
    /// Describes how a virtual file system service should determine which file to return
    /// when multiple matching candidates are found.
    /// </summary>
    public enum VfsFileResolutionMode
    {
        /// <summary>
        /// The file sources will be enumerated in priority order, and the first matching file will be returned.
        /// </summary>
        ReturnFirstMatch,
        /// <summary>
        /// All file sources will be enumerated, and the match with the most recent LastWriteTime will be returned.
        /// </summary>
        ReturnNewestMatch
    }

    /// <summary>
    /// Represents a virtual file system. A VFS will resolve all file petitions
    /// done through it masking the underlying details to the user.
    /// </summary>
    public interface IVfsService
    {
        #region Properties and Indexers
        /// <summary>
        /// Gets the file sources registered with the <see cref="IVfsService"/>.
        /// </summary>
        /// <value>The file sources.</value>
        IIndexedCollection<IFilesSource> Sources { get; }

        /// <summary>
        /// Gets the name of the <see cref="IVfsService"/>.
        /// </summary>
        /// <value>The name.</value>
        string Name { get; }

        /// <summary>
        /// Gets or sets the file resolution mode used by the <see cref="IVfsService"/>.
        /// </summary>
        /// <value>The file resolution mode.</value>
        VfsFileResolutionMode FileResolutionMode { get; set; }
        #endregion

        #region Methods
        /// <summary>
        /// Adds a new <see cref="IFilesSource"/> to the <see cref="IVfsService"/>.
        /// </summary>
        /// <param name="source"><see cref="IFilesSource"/> to add.</param>
        void AddSource(IFilesSource source);

        /// <summary>
        /// Gets a <see cref="IFilesSource"/>.
        /// </summary>
        /// <param name="sourceName">Name of the <see cref="IFilesSource"/> to get.</param>
        /// <returns>The selected <see cref="IFilesSource"/>.</returns>
        IFilesSource GetSource(string sourceName);

        /// <summary>
        /// Gets a <see cref="IWritableFilesSource"/>.
        /// </summary>
        /// <param name="sourceName">Name of the <see cref="IWritableFilesSource"/> to get.</param>
        /// <returns>The selected <see cref="IWritableFilesSource"/>.</returns>
        IWritableFilesSource GetWritableSource(string sourceName);

        /// <summary>
        /// Gets a handle to the requested file.
        /// </summary>
        /// <param name="virtualPath">The virtual path of the file.</param>
        /// <returns>A handle to the requested file.</returns>
        IVirtualFileInfo GetFile(string virtualPath);

        /// <summary>
        /// Gets a handle to the requested writeable file.
        /// </summary>
        /// <param name="virtualPath">The virtual path of the file.</param>
        /// <returns>A handle to the requested file.</returns>
        IVirtualFileInfo GetWriteableFile(string virtualPath);
        #endregion
    }
}