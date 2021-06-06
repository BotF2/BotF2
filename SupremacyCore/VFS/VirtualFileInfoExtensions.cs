using System;
using System.Collections.Generic;
using System.IO;
using System.Security.AccessControl;
using System.Text;
using System.Threading;

using Supremacy.Annotations;
using Supremacy.Utility;

namespace Supremacy.VFS
{
    /// <summary>
    /// Provides extension methods for the <see cref="IVirtualFileInfo"/> interface.
    /// </summary>
    public static class VirtualFileInfoExtensions
    {
#pragma warning disable 1591
        /// <summary>
        /// Gets a <see cref="FileSecurity"/> object that encapsulates the access control list (ACL) entries for the file.
        /// </summary>
        /// <returns>A <see cref="FileSecurity"/> object that encapsulates the access control rules for the file.</returns>
        /// <remarks>
        /// This method returns the <see cref="AccessControlSections.Group"/>, <see cref="AccessControlSections.Owner"/>,
        /// and <see cref="AccessControlSections.Access"/> sections of the access control list (ACL) entries for the file.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <param name="source"/> parameter is <c>null</c>.</exception>
        /// <exception cref="IOException">An I/O error occurred while opening the file.</exception>
        /// <exception cref="PlatformNotSupportedException">The current operating system does not support access control lists (ACLs).</exception>
        /// <exception cref="PrivilegeNotHeldException">The current system account does not have administrative privileges.</exception>
        /// <exception cref="SystemException">The file could not be found.</exception>
        /// <exception cref="UnauthorizedAccessException">This operation is not supported on the current platform, or the caller does not have the required permission.</exception>
        public static FileSecurity GetAccessControl([NotNull] this IVirtualFileInfo source)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            return source.GetAccessControl(AccessControlSections.Group | AccessControlSections.Owner | AccessControlSections.Access);
        }

        private static UTF8Encoding _utf8NoBOM;

        private static Encoding UTF8NoBOM
        {
            get
            {
                if (_utf8NoBOM != null)
                    return _utf8NoBOM;

                UTF8Encoding encoding = new UTF8Encoding(false, true);
                Thread.MemoryBarrier();
                _utf8NoBOM = encoding;
                return _utf8NoBOM;
            }
        }


        private static FileShare GetDefaultFileShare(FileAccess fileAccess)
        {
            switch (fileAccess)
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

        /// <summary>
        /// Opens a <see cref="Stream"/> on the file, with the specified access.
        /// </summary>
        /// <param name="self">The file.</param>
        /// <param name="fileAccess">A <see cref="FileAccess"/> value that specifies the operations that can be performed on the file.</param>
        /// <returns>A <see cref="Stream"/> that provides access to the file, with the specified access.</returns>
        public static Stream Open([NotNull] this IVirtualFileInfo self, FileAccess fileAccess)
        {
            if (self == null)
                throw new ArgumentNullException("self");
            return self.Open(fileAccess, GetDefaultFileShare(fileAccess));
        }

        /// <summary>
        /// Tries to open a <see cref="Stream"/> on the file, with the specified access
        /// without throwing any exceptions.
        /// </summary>
        /// <param name="self">The file.</param>
        /// <param name="fileAccess">A <see cref="FileAccess"/> value that specifies the operations that can be performed on the file.</param>
        /// <param name="stream">A <see cref="Stream"/> that provides access to the file, with the specified access.</param>
        /// <returns><c>true</c> if the file is successfully opened; otherwise, <c>false</c>.</returns>
        public static bool TryOpen([NotNull] this IVirtualFileInfo self, FileAccess fileAccess, out Stream stream)
        {
            return self.TryOpen(fileAccess, GetDefaultFileShare(fileAccess), out stream);
        }

        /// <summary>
        /// Opens a <see cref="Stream"/> on the file with read access.
        /// </summary>
        /// <param name="self">The file.</param>
        /// <returns>A <see cref="Stream"/> that provides access to the file with the read access.</returns>
        public static Stream OpenRead([NotNull] this IVirtualFileInfo self)
        {
            return self.Open(FileAccess.Read);
        }

        /// <summary>
        /// Opens a <see cref="Stream"/> on the file with read access with the specified sharing option
        /// without throwing any exceptions.
        /// </summary>
        /// <param name="self">The file.</param>
        /// <param name="fileShare">A <see cref="FileShare"/> value specifying the type of access other threads have to the file.</param>
        /// <param name="stream">A <see cref="Stream"/> that provides access to the file with the read access and the specified sharing option.</param>
        /// <returns><c>true</c> if the file is successfully opened; otherwise, <c>false</c>.</returns>
        public static bool TryOpenRead([NotNull] this IVirtualFileInfo self, FileShare fileShare, out Stream stream)
        {
            if (self == null)
                throw new ArgumentNullException("self");
            return self.TryOpen(FileAccess.Read, fileShare, out stream);
        }

        /// <summary>
        /// Opens a <see cref="Stream"/> on the file with read access without throwing any exceptions.
        /// </summary>
        /// <param name="self">The file.</param>
        /// <param name="stream">A <see cref="Stream"/> that provides access to the file with the read access and the specified sharing option.</param>
        /// <returns><c>true</c> if the file is successfully opened; otherwise, <c>false</c>.</returns>
        public static bool TryOpenRead([NotNull] this IVirtualFileInfo self, out Stream stream)
        {
            return self.TryOpenRead(GetDefaultFileShare(FileAccess.Read), out stream);
        }

        /// <summary>
        /// Opens an existing UTF-8 encoded text file for reading.
        /// </summary>
        /// <param name="self">The file.</param>
        /// <returns>A <see cref="StreamReader"/> on the file.</returns>
        public static StreamReader OpenText([NotNull] this IVirtualFileInfo self)
        {
            return new StreamReader(self.OpenRead());
        }

        /// <summary>
        /// Tries to open an existing UTF-8 encoded text file for reading without throwing any exceptions.
        /// </summary>
        /// <param name="self">The file.</param>
        /// <param name="reader">A <see cref="StreamReader"/> on the file.</param>
        /// <returns><c>true</c> if the file was opened successfully; otherwise, <c>false</c></returns>
        public static bool TryOpenText([NotNull] this IVirtualFileInfo self, out StreamReader reader)
        {
            try
            {
                reader = new StreamReader(self.OpenRead());
                return true;
            }
            catch
            {
                reader = null;
                return false;
            }
        }

        /// <summary>
        /// Opens an exiting file for writing.
        /// </summary>
        /// <param name="self">The file.</param>
        /// <returns>A <see cref="Stream"/> on the file with <see cref="FileAccess.Write"/> access.</returns>
        public static Stream OpenWrite([NotNull] this IVirtualFileInfo self)
        {
            return self.Open(FileAccess.Write);
        }

        /// <summary>
        /// Tries to open an exiting file for writing without throwing any exceptions.
        /// </summary>
        /// <param name="self">The file.</param>
        /// <param name="stream">A <see cref="Stream"/> that provides access to the file, with <see cref="FileAccess.Write"/> access.</param>
        /// <returns>A <see cref="Stream"/> on the file with <see cref="FileAccess.Write"/> access.</returns>
        public static bool TryOpenWrite([NotNull] this IVirtualFileInfo self, out Stream stream)
        {
            return self.TryOpen(FileAccess.Write, out stream);
        }

        /// <summary>
        /// Creates or overwrites the file and opens it for reading.
        /// </summary>
        /// <param name="self">The file.</param>
        /// <returns>A <see cref="Stream"/> on the file with <see cref="FileAccess.ReadWrite"/> access.</returns>
        public static Stream Create([NotNull] this IVirtualFileInfo self)
        {
            return self.Open(FileAccess.Write);
        }

        /// <summary>
        /// Tries to open a text file, read all lines of the file, and then close the file
        /// without throwing any exceptions.
        /// </summary>
        /// <param name="self">The file.</param>
        /// <param name="encoding">The encoding applied to the contents of the file.</param>
        /// <param name="text">A string containing all lines of the file.</param>
        /// <returns><c>true</c> if the file is successfully read; otherwisem, <c>false</c>.</returns>
        public static bool TryReadAllText([NotNull] this IVirtualFileInfo self, Encoding encoding, out string text)
        {
            try
            {
                Stream stream;
                if (!self.TryOpenRead(out stream))
                {
                    text = null;
                    return false;
                }
                using (StreamReader textReader = new StreamReader(stream, encoding))
                {
                    text = textReader.ReadToEnd();
                    return true;
                }
            }
            catch
            {
                text = null;
                return false;
            }
        }

        /// <summary>
        /// Tries to open a UTF-8 text file, read all lines of the file, and then close the file
        /// without throwing any exceptions.
        /// </summary>
        /// <param name="self">The file.</param>
        /// <param name="text">A string containing all lines of the file.</param>
        /// <returns><c>true</c> if the file is successfully read; otherwisem, <c>false</c>.</returns>
        public static bool TryReadAllText([NotNull] this IVirtualFileInfo self, out string text)
        {
            return self.TryReadAllText(UTF8NoBOM, out text);
        }

        /// <summary>
        /// Opens a UTF-8 text file, reads all lines of the file, and then closes the file.
        /// </summary>
        /// <param name="self">The file.</param>
        /// <returns>A string containing all lines of the file.</returns>
        public static string ReadAllText([NotNull] this IVirtualFileInfo self, [NotNull] Encoding encoding)
        {
            if (encoding == null)
                throw new ArgumentNullException("encoding");

            using (StreamReader textReader = new StreamReader(self.OpenRead(), encoding))
            {
                return textReader.ReadToEnd();
            }
        }

        /// <summary>
        /// Opens a UTF-8 text file, reads all lines of the file, and then closes the file.
        /// </summary>
        /// <param name="self">The file.</param>
        /// <returns>A string containing all lines of the file.</returns>
        public static string ReadAllText([NotNull] this IVirtualFileInfo self)
        {
            return self.ReadAllText(Encoding.UTF8);
        }

        /// <summary>
        /// Opens a file, reads all lines of the file, and then closes the file.
        /// </summary>
        /// <param name="self">The file.</param>
        /// <param name="encoding">The encoding applied to the contents of the file.</param>
        /// <returns>A string array containing all lines of the file.</returns>
        public static string[] ReadAllLines([NotNull] this IVirtualFileInfo self, Encoding encoding)
        {
            List<string> list = new List<string>();
            using (StreamReader textReader = new StreamReader(self.OpenRead(), encoding))
            {
                string line;
                while ((line = textReader.ReadLine()) != null)
                {
                    list.Add(line);
                }
            }
            return list.ToArray();
        }

        /// <summary>
        /// Opens a file, reads all lines of the file, and then closes the file.
        /// </summary>
        /// <param name="self">The file.</param>
        /// <returns>A string array containing all lines of the file.</returns>
        public static string[] ReadAllLines([NotNull] this IVirtualFileInfo self)
        {
            return self.ReadAllLines(Encoding.UTF8);
        }

        /// <summary>
        /// Tries to opens a UTF-8 text file, read all lines of the file, and then close the file
        /// without throwing any exceptions.
        /// </summary>
        /// <param name="self">The file.</param>
        /// <param name="lines">A string array containing all lines of the file.</param>
        /// <returns><c>true</c> if the file is successfully read; otherwisem, <c>false</c>.</returns>
        public static bool TryReadAllLines([NotNull] this IVirtualFileInfo self, out string[] lines)
        {
            return self.TryReadAllLines(UTF8NoBOM, out lines);
        }

        /// <summary>
        /// Tries to opens a file, read all lines of the file, and then close the file
        /// without throwing any exceptions.
        /// </summary>
        /// <param name="self">The file.</param>
        /// <param name="encoding">The encoding applied to the contents of the file.</param>
        /// <param name="lines">A string array containing all lines of the file.</param>
        /// <returns><c>true</c> if the file is successfully read; otherwisem, <c>false</c>.</returns>
        public static bool TryReadAllLines([NotNull] this IVirtualFileInfo self, Encoding encoding, out string[] lines)
        {
            try
            {
                Stream stream;
                if (!self.TryOpenRead(out stream))
                {
                    lines = null;
                    return false;
                }
                List<string> list = new List<string>();
                using (StreamReader textReader = new StreamReader(stream, encoding))
                {
                    string line;
                    while ((line = textReader.ReadLine()) != null)
                    {
                        list.Add(line);
                    }
                }
                lines = list.ToArray();
                return true;
            }
            catch
            {
                lines = null;
                return false;
            }
        }

        /// <summary>
        /// Creates a new UTF-8 text file, writes the specified string to the file, and then closes the file.
        /// If the target file already exists, it is overwritten.
        /// </summary>
        /// <param name="self">The file.</param>
        /// <param name="text">The string to write to the file.</param>
        public static void WriteAllText([NotNull] this IVirtualFileInfo self, string text)
        {
            self.WriteAllText(text, UTF8NoBOM);
        }

        /// <summary>
        /// Creates a new file, writes the specified string to the file, and then closes the file.
        /// If the target file already exists, it is overwritten.
        /// </summary>
        /// <param name="self">The file.</param>
        /// <param name="text">The string to write to the file.</param>
        /// <param name="encoding">An System.Text.Encoding object that represents the encoding to apply to the string.</param>
        public static void WriteAllText([NotNull] this IVirtualFileInfo self, string text, Encoding encoding)
        {
            if (self == null)
                throw new ArgumentNullException("self");
            using (StreamWriter textWriter = new StreamWriter(self.Create(), encoding))
            {
                textWriter.Write(text);
            }
        }

        /// <summary>
        /// Create a new file, writes the specified string array to the file, and then closes the file.
        /// If the target file already exists, it is overwritten.
        /// </summary>
        /// <param name="self">The file.</param>
        /// <param name="lines">The string array to write to the file.</param>
        /// <param name="encoding">An System.Text.Encoding object that represents the encoding to apply to the string array.</param>
        public static void WriteAllLines([NotNull] this IVirtualFileInfo self, string[] lines, Encoding encoding)
        {
            if (self == null)
                throw new ArgumentNullException("self");
            using (StreamWriter textWriter = new StreamWriter(self.Create(), encoding))
            {
                foreach (string line in lines)
                {
                    textWriter.WriteLine(line);
                }
            }
        }

        /// <summary>
        /// Create a new UTF-8 text file, writes the specified string array to the file, and then closes the file.
        /// If the target file already exists, it is overwritten.
        /// </summary>
        /// <param name="self">The file.</param>
        /// <param name="lines">The string array to write to the file.</param>
        public static void WriteAllLines([NotNull] this IVirtualFileInfo self, string[] lines)
        {
            self.WriteAllLines(lines, UTF8NoBOM);
        }

        /// <summary>
        /// Tries to create a new UTF-8 text file, write the specified string to the file, and then close the file
        /// without throwing any exceptions.  If the target file already exists, it is overwritten.
        /// </summary>
        /// <param name="self">The file.</param>
        /// <param name="text">The string to write to the file.</param>
        /// <returns><c>true</c> if the file is successfully written; otherwise, <c>false</c>.</returns>
        public static bool TryWriteAllText([NotNull] this IVirtualFileInfo self, string text)
        {
            return self.TryWriteAllText(text, UTF8NoBOM);
        }

        /// <summary>
        /// Tries to create a new file, write the specified string to the file, and then close the file
        /// without throwing any exceptions.  If the target file already exists, it is overwritten.
        /// </summary>
        /// <param name="self">The file.</param>
        /// <param name="text">The string to write to the file.</param>
        /// <param name="encoding">An System.Text.Encoding object that represents the encoding to apply to the string.</param>
        /// <returns><c>true</c> if the file is successfully written; otherwise, <c>false</c>.</returns>
        public static bool TryWriteAllText([NotNull] this IVirtualFileInfo self, string text, Encoding encoding)
        {
            try
            {
                self.WriteAllText(text, encoding);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Tries to create a new file, write the specified string array to the file, and then close the file
        /// without throwing any exceptions.  If the target file already exists, it is overwritten.
        /// </summary>
        /// <param name="self">The file.</param>
        /// <param name="lines">The string array to write to the file.</param>
        /// <param name="encoding">An System.Text.Encoding object that represents the encoding to apply to the string array.</param>
        /// <returns><c>true</c> if the file is successfully written; otherwise, <c>false</c>.</returns>
        public static bool TryWriteAllLines([NotNull] this IVirtualFileInfo self, string[] lines, Encoding encoding)
        {
            try
            {
                self.WriteAllLines(lines, encoding);
                return true;
            }
            catch
            {
                return false;
            }
        }

        // <summary>
        /// Tries to create a new file, write the specified string array to the file, and then close the file
        /// without throwing any exceptions.  If the target file already exists, it is overwritten.
        /// </summary>
        /// <param name="self">The file.</param>
        /// <param name="lines">The string array to write to the file.</param>
        /// <returns><c>true</c> if the file is successfully written; otherwise, <c>false</c>.</returns>
        public static bool TryWriteAllLines([NotNull] this IVirtualFileInfo self, string[] lines)
        {
            return self.TryWriteAllLines(lines, UTF8NoBOM);
        }

        /// <summary>
        /// Creates a new file, writes the specified byte array to the file, and then closes the file.
        /// If the target file already exists, it is overwritten.
        /// </summary>
        /// <param name="self">The file.</param>
        /// <param name="bytes">The bytes to write to the file.</param>
        public static void WriteAllBytes([NotNull] this IVirtualFileInfo self, [NotNull] byte[] bytes)
        {
            if (self == null)
                throw new ArgumentNullException("self");
            if (bytes == null)
                throw new ArgumentNullException("bytes");
            using (Stream writeStream = self.Create())
            {
                writeStream.Write(bytes, 0, bytes.Length);
            }
        }

        /// <summary>
        /// Tries to create a new file, write the specified byte array to the file, and then close the file
        /// without throwing any exceptions.  If the target file already exists, it is overwritten.
        /// </summary>
        /// <param name="self">The file.</param>
        /// <param name="bytes">The bytes to write to the file.</param>
        /// <returns><c>true</c> if the file is successfully written; otherwise, <c>false</c>.</returns>
        public static bool TryWriteAllBytes([NotNull] this IVirtualFileInfo self, [NotNull] byte[] bytes)
        {
            if (self == null)
                throw new ArgumentNullException("self");
            if (bytes == null)
                throw new ArgumentNullException("bytes");

            Stream writeStream;
            if (!self.TryCreate(out writeStream))
                return false;

            try
            {
                writeStream.Write(bytes, 0, bytes.Length);
                return true;
            }
            catch (Exception e)
            {
                GameLog.Core.General.Error(e);
            }
            finally
            {
                writeStream.Dispose();
            }

            return false;
        }

        /// <summary>
        /// Opens a binary file, reads the contents, and then closes the file.
        /// </summary>
        /// <param name="self">The file.</param>
        /// <returns>The contents of the file.</returns>
        public static byte[] ReadAllBytes([NotNull] this IVirtualFileInfo self)
        {
            if (self == null)
                throw new ArgumentNullException("self");
            byte[] buffer = new byte[self.Length];
            using (Stream readStream = self.OpenRead())
            {
                readStream.Read(buffer, 0, buffer.Length);
            }
            return buffer;
        }

        /// <summary>
        /// Tries to open a binary file, read the contents, and then close the file.
        /// </summary>
        /// <param name="self">The file.</param>
        /// <param name="bytes">The contents of the file.</param>
        /// <returns><c>true</c> if the file is successfully read; otherwise, <c>false</c>.</returns>
        public static bool TryReadAllBytes([NotNull] this IVirtualFileInfo self, out byte[] bytes)
        {
            if (self == null)
                throw new ArgumentNullException("self");
            Stream readStream;
            if (!self.TryOpenRead(out readStream))
            {
                bytes = null;
                return false;
            }
            try
            {
                using (readStream)
                {
                    bytes = new byte[self.Length];
                    readStream.Read(bytes, 0, bytes.Length);
                }
                return true;
            }
            catch
            {
                bytes = null;
                return false;
            }
        }
#pragma warning restore 1591
    }
}