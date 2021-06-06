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

using Supremacy.Utility;
using System;
using System.IO;

namespace Supremacy.VFS
{
    public class StreamDecorator<T> : Stream, IVirtualFileStreamInternal, IVirtualFileStream
        where T : Stream
    {
        #region Fields
        private T _baseStream;
        private bool _closed;
        private readonly string _resolvedPath;
        #endregion

        #region Events
        private FileAccess _access;
        private FileShare _share;
        public event EventHandler<EventArgs> Closed;
        #endregion

        #region Constructors
        public StreamDecorator(string resolvedPath, T baseStream, FileAccess access, FileShare share)
        {
            _resolvedPath = resolvedPath;
            BaseStream = baseStream;
            _share = share;
            _access = access;
        }
        #endregion

        #region Properties and Indexers
        protected T BaseStream
        {
            get { return _baseStream; }
            set
            {
                VerifyNotDisposed();
                if (ReferenceEquals(_baseStream, value))
                    return;
                if (_baseStream != null)
                    GC.ReRegisterForFinalize(_baseStream);
                _baseStream = value;
                if (_baseStream != null)
                    GC.SuppressFinalize(_baseStream);
            }
        }

        public override bool CanRead => !IsDisposed && _baseStream.CanRead;

        protected bool IsDisposed => _closed;

        public override bool CanSeek => !IsDisposed && _baseStream.CanSeek;

        public override bool CanWrite => !IsDisposed && _baseStream.CanWrite;

        public override long Length
        {
            get
            {
                VerifyNotDisposed();
                return _baseStream.Length;
            }
        }

        public override long Position
        {
            get
            {
                VerifyNotDisposed();
                return _baseStream.Position;
            }
            set
            {
                VerifyNotDisposed();
                _baseStream.Position = value;
            }
        }
        #endregion

        #region Methods
        public override void Flush()
        {
            VerifyNotDisposed();
            _baseStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            VerifyNotDisposed();
            if (!CanRead)
                throw new NotSupportedException("Stream does not support read.");
            return _baseStream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            VerifyNotDisposed();
            return _baseStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            VerifyNotDisposed();
            _baseStream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            VerifyNotDisposed();
            _baseStream.Write(buffer, offset, count);
        }

        protected override void Dispose(bool disposing)
        {
            if (_closed)
                return;

            try
            {
                base.Dispose(disposing);
            }
            catch (Exception e)
            {
                GameLog.Core.General.Error(e);
            }

            var baseStream = _baseStream;
            if (baseStream != null)
            {
                if (disposing)
                {
                    try
                    {
                        _baseStream.Close();
                    }
                    catch (Exception e)
                    {
                        GameLog.Core.General.Error(e);
                    }
                }
                else
                {
                    GC.ReRegisterForFinalize(_baseStream);
                }
            }

            OnClosed();

            _closed = true;

            if (disposing)
                GC.SuppressFinalize(this);
        }

        protected virtual void OnClosed()
        {
            Console.WriteLine("Close called for file {0}", ResolvedPath);
            if (Closed != null)
                Closed(this, EventArgs.Empty);
        }

        protected void VerifyNotDisposed()
        {
            if (_closed)
                throw new ObjectDisposedException("StreamDecorator");
        }

        ~StreamDecorator()
        {
            Dispose(false);
        }
        #endregion

        #region IVirtualFileStream Members
        public string SourceName { get; protected internal set; }
        public string VirtualPath { get; protected internal set; }

        string IVirtualFileStreamInternal.ResolvedPath => ResolvedPath;

        protected internal string ResolvedPath => _resolvedPath;

        public FileAccess Access => _access;

        public FileShare Share => _share;
        #endregion
    }
}