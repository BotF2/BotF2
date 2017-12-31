namespace Supremacy.VFS
{
    /// <summary>
    /// Provides some additional metadata about a virtual file stream for internal use.
    /// </summary>
    internal interface IVirtualFileStreamInternal : IVirtualFileStream
    {
        /// <summary>
        /// Gets the resolved (physical) path of the file referenced by the stream.
        /// </summary>
        /// <value>The resolved (physical) path of the file referenced by the stream.</value>
        string ResolvedPath { get; }
    }
}