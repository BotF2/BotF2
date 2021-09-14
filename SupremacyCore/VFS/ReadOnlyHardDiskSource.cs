namespace Supremacy.VFS
{
    /// <summary>
    /// Represents a read-only <see cref="HardDiskSource"/> implementation that prevents
    /// files from being created, deleted, or written.
    /// </summary>
    public class ReadOnlyHardDiskSource : HardDiskSource
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReadOnlyHardDiskSource"/> class.
        /// </summary>
        /// <param name="name">The name of the source.</param>
        /// <param name="path">The base path of the source.</param>
        public ReadOnlyHardDiskSource(string name, string path) : base(name, path) { }

        public sealed override bool IsReadOnly => true;
    }
}