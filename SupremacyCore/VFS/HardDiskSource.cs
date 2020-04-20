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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.AccessControl;

using Supremacy.Annotations;

using IOPath = System.IO.Path;
using Supremacy.Utility;
using System.Windows;

namespace Supremacy.VFS
{
    /// <summary>
    /// Represents a path on a local hard disk (the most usual type of <see cref="IFilesSource"/>).
    /// </summary>
    public class HardDiskSource : AbstractWritableSource<Stream>
    {
        #region Fields
        private string _path;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="HardDiskSource"/> class.
        /// </summary>
        /// <param name="name">The name of the <see cref="HardDiskSource"/>.</param>
        /// <param name="path">The path of the <see cref="HardDiskSource"/>.</param>
        public HardDiskSource(string name, string path)
        {
            VerifySecure(path);

            Name = name;
            _path = path;
            _pathResolver = () => _path;
        }
        #endregion

        #region Properties and Indexers
        /// <summary>
        /// Gets the path of the <see cref="IFilesSource"/>.
        /// </summary>
        /// <value></value>
        public string Path
        {
            get
            {
                var path = _pathResolver();
                VerifySecure(path);
                return path;
            }
            set { _path = value; }
        }

        private Func<string> _pathResolver;
        public Func<string> PathResolver
        {
            get { return _pathResolver; }
            set { _pathResolver = value ?? (() => _path); }
        }

        public override bool IsReadOnly
        {
            get { return false; }
        }
        #endregion

        #region Methods
        public override void AddDefinedPath(string definedPathAlias, string path)
        {
            VerifySecure(path);
            base.AddDefinedPath(definedPathAlias, _path);
        }

        public override Stream CreateFile(string path)
        {
            VerifyNotReadOnly();
            VerifySecure(path);
            return BaseCreateStream(path, true);
        }

        protected void VerifyNotReadOnly()
        {
            if (IsReadOnly)
                throw new NotSupportedException("This source is read-only.");
        }

        public override Stream GetFile(string path, bool recurse)
        {
            if (string.IsNullOrEmpty(Path))
                return null;

            VerifySecure(path);
            return BaseGetStream(path, recurse, FileAccess.Read, true);
        }

        public override ReadOnlyCollection<string> GetFiles(string path, bool recurse, string searchPattern)
        {
            var files = new List<String>();

            if (string.IsNullOrEmpty(Path))
                return files.AsReadOnly();

            VerifySecure(path);

            var sourcePath = Path;
            var fullPath = IOPath.Combine(sourcePath, path);

            if (!Directory.Exists(fullPath))
                return files.AsReadOnly();

            // Get the files names
            String[] directoryFiles;
            if (recurse)
                directoryFiles = Directory.GetFiles(fullPath, searchPattern, SearchOption.AllDirectories);
            else
                directoryFiles = Directory.GetFiles(fullPath, searchPattern, SearchOption.TopDirectoryOnly);

            files.AddRange(directoryFiles.Select(f => IOPath.IsPathRooted(f) ? f.Remove(0, sourcePath.Length + 1) : f));

            return files.AsReadOnly();
        }

        public override Stream GetWritableFile(string path, bool recurse)
        {
            if (string.IsNullOrEmpty(Path))
                return null;

            VerifyNotReadOnly();
            VerifySecure(path);
            return BaseGetStream(path, recurse, FileAccess.ReadWrite, true);
        }

        public override bool RemoveFile(string path, bool recurse)
        {
            VerifyNotReadOnly();

            if (string.IsNullOrEmpty(Path))
                return false;

            VerifySecure(path);
            return BaseDeleteStream(path, recurse, true);
        }

        public override bool TryCreateFile(string path, out Stream stream)
        {
            if (string.IsNullOrEmpty(Path))
            {
                stream = null;
                return false;
            }

            if (IsReadOnly || !CheckPathValid(path))
            {
                stream = null;
                return false;
            }

            try
            {
                stream = BaseCreateStream(path, false);
            }
            catch
            {
                stream = null;
            }

            return stream != null;
        }

        public override bool TryGetFile(string path, bool recurse, out Stream stream)
        {
            if (string.IsNullOrEmpty(Path) || IsReadOnly || !CheckPathValid(path))
            {
                stream = null;
                return false;
            }

            try
            {
                stream = BaseGetStream(path, recurse, FileAccess.Read, false);
            }
            catch
            {
                stream = null;
            }

            return stream != null;
        }

        public override bool TryGetWritableFile(string path, bool recurse, out Stream stream)
        {
            if (string.IsNullOrEmpty(Path) || IsReadOnly || !CheckPathValid(path))
            {
                stream = null;
                return false;
            }

            try
            {
                stream = BaseGetStream(path, recurse, FileAccess.ReadWrite, false);
            }
            catch
            {
                stream = null;
            }

            return stream != null;
        }

        public override bool TryRemoveFile(string path, bool recurse)
        {
            if (string.IsNullOrEmpty(Path) || IsReadOnly || !CheckPathValid(path))
            {
                return false;
            }

            try
            {
                return BaseDeleteStream(path, recurse, false);
            }
            catch
            {
                return false;
            }
        }

        protected override Stream InternalCreateFile(string resolvedName)
        {
            VerifyNotReadOnly();

            if (string.IsNullOrEmpty(Path))
                return null;

            EnsureDirectory(resolvedName);
            return File.Open(resolvedName, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
        }

        protected void EnsureDirectory([NotNull] string path)
        {
            if (path == null)
                throw new ArgumentNullException("path");
            var directoryName = IOPath.GetDirectoryName(path);
            if (!Directory.Exists(directoryName))
                Directory.CreateDirectory(directoryName);
        }

        protected override void InternalDeleteFile(string resolvedName)
        {
            VerifyNotReadOnly();

            if (string.IsNullOrEmpty(Path))
                return;

            File.Delete(resolvedName);
        }

        protected override Stream InternalGetFile(string resolvedName, FileAccess access, FileShare share)
        {
            if ((access & FileAccess.Write) == FileAccess.Write)
                VerifyNotReadOnly();
            try
            {
                return File.Open(resolvedName, FileMode.Open, access, share);
            }
            catch
            {
                string message = "File is NOT available > " + resolvedName;
                MessageBox.Show(message, "WARNING", MessageBoxButton.OK);
                return File.Open("vfs:///Resources/Images/__image_missing.png", FileMode.Open, access, share);
            }
        }

        public override IVirtualFileInfo GetFileInfo(string path, bool recurse)
        {
            FileInfo fileInfo;

            if (string.IsNullOrEmpty(Path) || !CheckPathValid(path))
                return null;
            
            var resolvedName = ResolveFileName(path, recurse);

            if (string.IsNullOrEmpty(resolvedName))
                fileInfo = new FileInfo(TranslatePath(path));
            else
                fileInfo = new FileInfo(resolvedName);

            try
            {
                return new HardDiskVirtualFileInfo(
                    this,
                    path,
                    fileInfo);
            }
            catch (Exception e)
            {
                GameLog.Core.General.Error(e);
            }

            return null;
        }

        protected string TranslatePath(string path)
        {
            if (IOPath.IsPathRooted(path))
                return path;
            return IOPath.Combine(Path, path);
        }

        protected override string ResolveFileName(string path, bool recurse)
        {
            if (string.IsNullOrEmpty(Path))
                return string.Empty;

            path = TranslatePath(path);

            if (!recurse)
            {
                if (File.Exists(path))
                    return new FileInfo(path).FullName;
            }
            else
            {
                string dir = IOPath.GetDirectoryName(path);

                if (!Directory.Exists(dir))
                    return string.Empty;

                var info = new DirectoryInfo(dir);
                FileInfo[] files = info.GetFiles(IOPath.GetFileName(path), SearchOption.AllDirectories);

                if (files.Length > 0)
                    return files[0].FullName;
            }

            return string.Empty;
        }

        protected static void VerifySecure(string path)
        {
            if (path == null)
                return;
            if (path.Contains(".."))
                throw new SecurityException("A secure path can't contain the \"..\" modifier.");
        }

        protected static bool CheckPathValid(string path)
        {
            return !string.IsNullOrEmpty(path) && !path.Contains("..");
        }
        #endregion

        #region HardDiskVirtualFileInfo Class
        /// <summary>
        /// Provides an <see cref="IVirtualFileInfo"/> implementation for files in a <see cref="HardDiskSource"/>.
        /// </summary>
        protected class HardDiskVirtualFileInfo : IVirtualFileInfo
        {
            private readonly FileInfo _physicalFileInfo;
            private readonly HardDiskSource _source;
            private readonly string _virtualPath;

            /// <summary>
            /// Initializes a new instance of the <see cref="HardDiskVirtualFileInfo"/> class.
            /// </summary>
            /// <param name="source">The <see cref="HardDiskSource"/> that owns the file.</param>
            /// <param name="virtualPath">The file's virtual path.</param>
            /// <param name="physicalFileInfo">The physical file info.</param>
            public HardDiskVirtualFileInfo(
                [NotNull] HardDiskSource source,
                [NotNull] string virtualPath,
                [NotNull] FileInfo physicalFileInfo)
            {
                if (source == null)
                    throw new ArgumentNullException("source");
                if (virtualPath == null)
                    throw new ArgumentNullException("virtualPath");
                if (physicalFileInfo == null)
                    throw new ArgumentNullException("physicalFileInfo");
                _source = source;
                _virtualPath = virtualPath;
                _physicalFileInfo = physicalFileInfo;
            }

            #region IVirtualFileInfo Members
            public IFilesSource Source
            {
                get { return _source; }
            }

            /// <summary>
            /// Gets a value indicating whether the file exists.
            /// </summary>
            /// <value><c>true</c> if the gile exists; otherwise, <c>false</c>.</value>
            public bool Exists
            {
                get { return _physicalFileInfo.Exists; }
            }

            /// <summary>
            /// Gets a value indicating whether the file is read only.
            /// </summary>
            /// <value><c>true</c> if the file is read only; otherwise, <c>false</c>.</value>
            public bool IsReadOnly
            {
                get { return _source.IsReadOnly; }
            }

            /// <summary>
            /// Gets the file's virtual path.
            /// </summary>
            /// <value>The file's virtual path.</value>
            public string VirtualPath
            {
                get { return _virtualPath; }
            }

            /// <summary>
            /// Gets the length of the file in bytes.
            /// </summary>
            /// <value>The length of the file in bytes.</value>
            public long Length
            {
                get { return _physicalFileInfo.Length; }
            }

            /// <summary>
            /// Gets the name of the file without the directory name.
            /// </summary>
            /// <value>The name of the file.</value>
            public string FileName
            {
                get { return IOPath.GetFileName(_physicalFileInfo.FullName); }
            }

            /// <summary>
            /// Gets the name of the virtual directory containing the file.
            /// </summary>
            /// <value>The name of the virtual directory containing the file.</value>
            public string DirectoryName
            {
                get { return IOPath.GetDirectoryName(_physicalFileInfo.FullName); }
            }

            /// <summary>
            /// Gets the file extension.
            /// </summary>
            /// <value>The file extension.</value>
            public string Extension
            {
                get { return IOPath.GetExtension(_physicalFileInfo.FullName); }
            }

            /// <summary>
            /// Gets or sets the file's attributes.
            /// </summary>
            /// <value>The file's attributes.</value>
            public FileAttributes Attributes
            {
                get { return _physicalFileInfo.Attributes; }
                set
                {
                    VerifyNotReadOnly();
                    _physicalFileInfo.Attributes = value;
                }
            }

            /// <summary>
            /// Gets or sets the file creation time in coordinated universal time (UTC) format.
            /// </summary>
            /// <value>The UTC date and time that the file was created.</value>
            public DateTime CreationTimeUtc
            {
                get { return _physicalFileInfo.CreationTimeUtc; }
                set
                {
                    VerifyNotReadOnly();
                    _physicalFileInfo.CreationTimeUtc = value;
                }
            }

            /// <summary>
            /// Gets or sets the time, in coordinated universal time (UTC) format, that the file was last accessed.
            /// </summary>
            /// <value>The UTC date and time that the file was last accessed.</value>
            public DateTime LastAccessTimeUtc
            {
                get { return _physicalFileInfo.LastAccessTimeUtc; }
                set
                {
                    VerifyNotReadOnly();
                    _physicalFileInfo.LastAccessTimeUtc = value;
                }
            }

            /// <summary>
            /// Gets or sets the time, in coordinated universal time (UTC) format, that the file was last written to.
            /// </summary>
            /// <value>The UTC date and time that the file was last written to.</value>
            public DateTime LastWriteTimeUtc
            {
                get { return _physicalFileInfo.LastWriteTimeUtc; }
                set
                {
                    VerifyNotReadOnly();
                    _physicalFileInfo.LastWriteTimeUtc = value;
                }
            }

            /// <summary>
            /// Gets a <see cref="FileSecurity"/> object that encapsulates the specified type of access control list (ACL) entries for the file.
            /// </summary>
            /// <param name="includeSections">The group of access control entries to retrieve.</param>
            /// <returns>
            /// A <see cref="FileSecurity"/> object that encapsulates the access control rules for the file.
            /// </returns>
            /// <exception cref="IOException">An I/O error occurred while opening the file.</exception>
            /// <exception cref="PlatformNotSupportedException">The current operating system does not support access control lists (ACLs).</exception>
            /// <exception cref="PrivilegeNotHeldException">The current system account does not have administrative privileges.</exception>
            /// <exception cref="SystemException">The file could not be found.</exception>
            /// <exception cref="UnauthorizedAccessException">This operation is not supported on the current platform, or the caller does not have the required permission.</exception>
            public FileSecurity GetAccessControl(AccessControlSections includeSections)
            {
                return _physicalFileInfo.GetAccessControl(includeSections);
            }

            /// <summary>
            /// Applies access control list (ACL) entries described by <paramref name="fileSecurity"/> to the file.
            /// </summary>
            /// <param name="fileSecurity">A <see cref="FileSecurity"/> object that describes an access control list (ACL) entry to apply to the file.</param>
            /// <exception cref="ArgumentNullException">The <paramref name="fileSecurity"/> parameter is <c>null</c>.</exception>
            /// <exception cref="SystemException">The file could not be found or modified.</exception>
            /// <exception cref="UnauthorizedAccessException"></exception>
            /// <exception cref="PlatformNotSupportedException">The current operating system does not support access control lists (ACLs).</exception>
            public void SetAccessControl(FileSecurity fileSecurity)
            {
                VerifyNotReadOnly();
                _physicalFileInfo.SetAccessControl(fileSecurity);
            }

            /// <summary>
            /// Refreshes the file information in this <see cref="IVirtualFileInfo"/>.
            /// </summary>
            public void Refresh()
            {
                _physicalFileInfo.Refresh();
            }

            /// <summary>
            /// Opens the file with the specifid access and sharing levels.
            /// </summary>
            /// <param name="access">The access level desired.</param>
            /// <param name="share">The sharing level to be imposed on the file while the stream is open.</param>
            /// <returns>
            /// The file opened with the specified access and sharing levels.
            /// </returns>
            public Stream Open(FileAccess access, FileShare share)
            {
                var stream = _source.InternalGetFile(_physicalFileInfo.FullName, access, share);
                if ((access & FileAccess.Write) == FileAccess.Write)
                {
                    var virtualFileStream = new StreamDecorator<Stream>(
                        _physicalFileInfo.FullName,
                        _source.InternalGetFile(_physicalFileInfo.FullName, access, share),
                        access,
                        share);
                    virtualFileStream.Closed += OnStreamClosed;
                    stream = virtualFileStream;
                }
                Refresh();
                return stream;
            }

            private void OnStreamClosed(object sender, EventArgs args)
            {
                var virtualFileStream = sender as IVirtualFileStream;
                
                if (virtualFileStream != null)
                    virtualFileStream.Closed -= OnStreamClosed;

                Refresh();
            }

            /// <summary>
            /// Tries the open the file with the specifid access and sharing levels without throwing any exceptions.
            /// </summary>
            /// <param name="access">The access level desired.</param>
            /// <param name="share">The sharing level to be imposed on the file while the stream is open.</param>
            /// <param name="stream">The file opened with the specified access and sharing levels.</param>
            /// <returns>
            /// <c>true</c> if the file was successfully opened; otherwise, <c>false</c>.
            /// </returns>
            public bool TryOpen(FileAccess access, FileShare share, out Stream stream)
            {
                try
                {
                    stream = Open(access, share);
                    return true;
                }
                catch
                {
                    stream = null;
                    return false;
                }
            }

            /// <summary>
            /// Deletes the file.
            /// </summary>
            public void Delete()
            {
                _source.InternalDeleteFile(_physicalFileInfo.FullName);
                Refresh();
            }

            /// <summary>
            /// Tries to delete the file without throwing any exceptions.
            /// </summary>
            /// <returns>
            /// <c>true</c> if the file was successfully deleted; otherwise, <c>false</c>.
            /// </returns>
            public bool TryDelete()
            {
                try
                {
                    Delete();
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            /// <summary>
            /// Creates or overwrites the file.
            /// </summary>
            /// <returns>The newly created file.</returns>
            public Stream Create()
            {
                var stream = _source.InternalCreateFile(_physicalFileInfo.FullName);

                var createOnWriteStream = stream as CreateOnWriteHardDiskSource.CreateOnWriteStream;
                if (createOnWriteStream != null)
                {
                    createOnWriteStream.BaseStreamCreated += OnCreateOnWriteStreamOnBaseStreamCreated;
                    createOnWriteStream.Closed += OnStreamClosed;
                }

                Refresh();

                return stream;
            }

            private void OnCreateOnWriteStreamOnBaseStreamCreated(object sender, EventArgs args)
            {
                var createOnWriteStream = sender as CreateOnWriteHardDiskSource.CreateOnWriteStream;
                if (createOnWriteStream == null)
                    return;

                createOnWriteStream.BaseStreamCreated -= OnCreateOnWriteStreamOnBaseStreamCreated;

                Refresh();
            }

            /// <summary>
            /// Tries to create or overwrite a file without throwing any exceptions.
            /// </summary>
            /// <param name="stream">The newly created file.</param>
            /// <returns>
            /// <c>true</c> if the file was successfully created; otherwise, <c>false</c>
            /// </returns>
            public bool TryCreate(out Stream stream)
            {
                try
                {
                    stream = Create();
                    return true;
                }
                catch
                {
                    stream = null;
                    return false;
                }
            }
            #endregion

            /// <summary>
            /// Throws a <see cref="NotSupportedException"/> if this instance is read-only.
            /// </summary>
            protected void VerifyNotReadOnly()
            {
                if (!IsReadOnly)
                    return;
             
                Refresh();
                
                if (IsReadOnly)
                    throw new NotSupportedException("This virtual file is read-only.");
            }

            public bool Equals(IVirtualFileInfo other)
            {
                if (ReferenceEquals(other, this))
                    return true;

                var otherInfo = other as HardDiskVirtualFileInfo;
                if (ReferenceEquals(otherInfo, null))
                    return false;

                return _source.StringComparer.Equals(
                    otherInfo._physicalFileInfo.FullName,
                    _physicalFileInfo.FullName);
            }
        }
        #endregion
    }
}