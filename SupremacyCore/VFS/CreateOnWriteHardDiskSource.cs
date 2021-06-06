using System;
using System.Collections.Generic;
using System.IO;

//using Supremacy.Annotations;

using System.Linq;

using Supremacy.Annotations;

namespace Supremacy.VFS
{
    /// <summary>
    /// Represents a <see cref="HardDiskSource"/> with create-on-write behavior.
    /// </summary>
    /// <remarks>
    /// When a file is created, the stream returned will not actually create the physical file
    /// until the stream is written, flushed, or the length of the file is set.  If the stream is
    /// closed without modification, then a physical file will not be created, and if an existing
    /// file exists, it will not be overwritten.
    /// </remarks>
    public class CreateOnWriteHardDiskSource : HardDiskSource
    {
        private readonly object _createStreamsLock;
        private readonly List<CreateOnWriteStream> _createStreams;

        protected override bool CanAccessFile(string path, FileShare minimumShareLevel)
        {
            lock (_createStreamsLock)
            {
                CreateOnWriteStream openCreateStream = _createStreams.FirstOrDefault(o => StringComparer.Equals(o.VirtualPath, path));
                if (openCreateStream != null)
                    return false;
                return base.CanAccessFile(path, minimumShareLevel);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateOnWriteHardDiskSource"/> class.
        /// </summary>
        /// <param name="name">The name of the source.</param>
        /// <param name="path">The base path of the source.</param>
        public CreateOnWriteHardDiskSource(string name, string path) : base(name, path)
        {
            _createStreamsLock = new object();
            _createStreams = new List<CreateOnWriteStream>();
        }

        protected override sealed Stream InternalCreateFile(string resolvedName)
        {
            CreateOnWriteStream stream;
            lock (_createStreamsLock)
            {
                if (_createStreams.Any(o => StringComparer.Equals(o.ResolvedPath, resolvedName)))
                    throw new UnauthorizedAccessException("File is in use.");
                EnsureDirectory(resolvedName);
                stream = new CreateOnWriteStream(resolvedName);
                _createStreams.Add(stream);
            }
            stream.Closed += OnCreateStreamClosed;
            return stream;
        }

        private void OnCreateStreamClosed(object sender, EventArgs args)
        {
            EndCreateStreamTracking((CreateOnWriteStream) sender);
        }

        private void EndCreateStreamTracking([NotNull] CreateOnWriteStream stream)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");
            lock (_createStreamsLock)
            {
                _createStreams.Remove(stream);
            }
        }

        #region CreateOnWriteStream Class
        protected internal sealed class CreateOnWriteStream : StreamDecorator<FileStream>
        {
            private readonly object _createStreamLock;
            private volatile bool _isStreamCreated;

            public event EventHandler BaseStreamCreated;

            public CreateOnWriteStream(string resolvedPath)
                : base(resolvedPath, null, FileAccess.Write, FileShare.None)
            {
                _createStreamLock = new object();
            }

            public override bool CanRead => false;

            public override bool CanSeek => !IsDisposed;

            public override bool CanWrite => !IsDisposed;

            public override long Length
            {
                get
                {
                    if (_isStreamCreated)
                        return base.Length;
                    lock (_createStreamLock)
                    {
                        if (_isStreamCreated)
                            return base.Length;
                        return 0;
                    }
                }
            }

            public override long Position
            {
                get
                {
                    if (_isStreamCreated)
                        return base.Position;
                    lock (_createStreamLock)
                    {
                        if (_isStreamCreated)
                            return base.Position;
                        return 0;
                    }
                }
                set
                {
                    EnsureBaseStream();
                    base.Position = value;
                }
            }

            private void EnsureBaseStream()
            {
                if (_isStreamCreated)
                    return;
                lock (_createStreamLock)
                {
                    if (_isStreamCreated)
                        return;
                    VerifyNotDisposed();
                    BaseStream = File.Open(ResolvedPath, FileMode.Create, Access, Share);
                    _isStreamCreated = true;

                    EventHandler handler = BaseStreamCreated;
                    if (handler != null)
                        handler(this, EventArgs.Empty);
                }
            }

            public override void Flush()
            {
                VerifyNotDisposed();
                EnsureBaseStream();
                base.Flush();
            }

            public override void SetLength(long value)
            {
                EnsureBaseStream();
                base.SetLength(value);
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                EnsureBaseStream();
                base.Write(buffer, offset, count);
            }

            public override void WriteByte(byte value)
            {
                EnsureBaseStream();
                base.WriteByte(value);
            }

            public override IAsyncResult BeginWrite(
                byte[] buffer, int offset, int count, AsyncCallback callback, object state)
            {
                EnsureBaseStream();
                return base.BeginWrite(buffer, offset, count, callback, state);
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                EnsureBaseStream();
                return base.Seek(offset, origin);
            }
        }
        #endregion
    }
}