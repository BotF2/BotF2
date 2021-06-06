using System;
using System.IO;

namespace Supremacy.VFS
{
    /// <summary>
    /// Provides some basic metadata about a virtual file stream.
    /// </summary>
    public interface IVirtualFileStream
    {
        #region Events
        /// <summary>
        /// Occurs when the stream is closed.
        /// </summary>
        event EventHandler<EventArgs> Closed;
        #endregion

        #region Properties
        /// <summary>
        /// Gets the name of the <see cref="IFilesSource"/> that owns the file stream.
        /// </summary>
        /// <value>The name of the <see cref="IFilesSource"/> that owns the file stream.</value>
        string SourceName { get; }

        /// <summary>
        /// Gets the virtual path of the file referenced by the stream.
        /// </summary>
        /// <value>The virtual path of the file referenced by the stream.</value>
        string VirtualPath { get; }

        /// <summary>
        /// Gets the file access granted to the stream.
        /// </summary>
        /// <value>The file access granted to the stream.</value>
        FileAccess Access { get; }

        /// <summary>
        /// Gets the file sharing level imposed by the stream.
        /// </summary>
        /// <value>The file sharing level imposed by the stream.</value>
        FileShare Share { get; }
        #endregion
    }
}