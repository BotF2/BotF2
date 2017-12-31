using System.IO;

namespace Supremacy.VFS
{
    /// <summary>
    /// Represents a virtual file stream for a <see cref="HardDiskSource"/>.
    /// </summary>
    public class HardDiskVirtualFileStream : StreamDecorator<Stream>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HardDiskVirtualFileStream"/> class.
        /// </summary>
        /// <param name="resolvedPath">The resolved path.</param>
        /// <param name="baseStream">The base stream.</param>
        /// <param name="access">The file access level.</param>
        /// <param name="share">The file sharing level.</param>
        public HardDiskVirtualFileStream(string resolvedPath, Stream baseStream, FileAccess access, FileShare share)
            : base(resolvedPath, baseStream, access, share) {}
    }
}