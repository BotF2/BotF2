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
using System.IO;
using System.Security.AccessControl;

namespace Supremacy.VFS
{
    /// <summary>
    /// Provides standard metdata about a file in a virtual file system.
    /// </summary>
    public interface IVirtualFileInfo : IEquatable<IVirtualFileInfo>
    {
        /// <summary>
        /// Gets the <see cref="IFilesSource"/> that owns the file.
        /// </summary>
        /// <value>The <see cref="IFilesSource"/> that owns the file.</value>
        IFilesSource Source { get; }

        /// <summary>
        /// Gets a value indicating whether the file exists.
        /// </summary>
        /// <value><c>true</c> if the gile exists; otherwise, <c>false</c>.</value>
        bool Exists { get; }

        /// <summary>
        /// Gets a value indicating whether the file is read only.
        /// </summary>
        /// <value>
        /// <c>true</c> if the file is read only; otherwise, <c>false</c>.
        /// </value>
        bool IsReadOnly { get; }

        /// <summary>
        /// Gets the file's virtual path.
        /// </summary>
        /// <value>The file's virtual path.</value>
        string VirtualPath { get; }

        /// <summary>
        /// Gets the length of the file in bytes.
        /// </summary>
        /// <value>The length of the file in bytes.</value>
        long Length { get; }

        /// <summary>
        /// Gets the name of the file without the directory name.
        /// </summary>
        /// <value>The name of the file.</value>
        string FileName { get; }

        /// <summary>
        /// Gets the name of the virtual directory containing the file.
        /// </summary>
        /// <value>The name of the virtual directory containing the file.</value>
        string DirectoryName { get; }

        /// <summary>
        /// Gets the file extension.
        /// </summary>
        /// <value>The file extension.</value>
        string Extension { get; }

        /// <summary>
        /// Gets or sets the file's attributes.
        /// </summary>
        /// <value>The file's attributes.</value>
        FileAttributes Attributes { get; set; }

        /// <summary>
        /// Gets or sets the file creation time in coordinated universal time (UTC) format.
        /// </summary>
        /// <value>The UTC date and time that the file was created.</value>
        DateTime CreationTimeUtc { get; set; }

        /// <summary>
        /// Gets or sets the time, in coordinated universal time (UTC) format, that the file was last accessed.
        /// </summary>
        /// <value>The UTC date and time that the file was last accessed.</value>
        DateTime LastAccessTimeUtc { get; set; }

        /// <summary>
        /// Gets or sets the time, in coordinated universal time (UTC) format, that the file was last written to.
        /// </summary>
        /// <value>The UTC date and time that the file was last written to.</value>
        DateTime LastWriteTimeUtc { get; set; }

        /// <summary>
        /// Gets a <see cref="FileSecurity"/> object that encapsulates the specified type of access control list (ACL) entries for the file.
        /// </summary>
        /// <param name="includeSections">The group of access control entries to retrieve.</param>
        /// <returns>A <see cref="FileSecurity"/> object that encapsulates the access control rules for the file.</returns>
        FileSecurity GetAccessControl(AccessControlSections includeSections);

        /// <summary>
        /// Applies access control list (ACL) entries described by <paramref name="fileSecurity"/> to the file.
        /// </summary>
        /// <param name="fileSecurity">A <see cref="FileSecurity"/> object that describes an access control list (ACL) entry to apply to the file.</param>
        void SetAccessControl(FileSecurity fileSecurity);

        /// <summary>
        /// Refreshes the file information in this <see cref="IVirtualFileInfo"/>.
        /// </summary>
        void Refresh();

        /// <summary>
        /// Opens the file with the specifid access and sharing levels.
        /// </summary>
        /// <param name="access">The access level desired.</param>
        /// <param name="share">A <see cref="FileShare"/> value specifying the type of access other threads have to the file.</param>
        /// <returns>The file opened with the specified access and sharing levels.</returns>
        Stream Open(FileAccess access, FileShare share);

        /// <summary>
        /// Tries the open the file with the specifid access and sharing levels without throwing any exceptions.
        /// </summary>
        /// <param name="access">The access level desired.</param>
        /// <param name="share">The sharing level to be imposed on the file while the stream is open.</param>
        /// <param name="stream">The file opened with the specified access and sharing levels.</param>
        /// <returns><c>true</c> if the file was successfully opened; otherwise, <c>false</c>.</returns>
        bool TryOpen(FileAccess access, FileShare share, out Stream stream);

        /// <summary>
        /// Deletes the file.
        /// </summary>
        void Delete();

        /// <summary>
        /// Tries to delete the file without throwing any exceptions.
        /// </summary>
        /// <returns><c>true</c> if the file was successfully deleted; otherwise, <c>false</c>.</returns>
        bool TryDelete();

        // <summary>
        /// Creates or overwrites the file and opens it for reading.
        /// </summary>
        /// <returns>A <see cref="Stream"/> on the file with <see cref="FileAccess.ReadWrite"/> access.</returns>
        Stream Create();

        /// <summary>
        /// Tries to create or overwrite a file without throwing any exceptions.
        /// </summary>
        /// <param name="stream">The newly created file.</param>
        /// <returns><c>true</c> if the file was successfully created; otherwise, <c>false</c></returns>
        bool TryCreate(out Stream stream);
    }
}
