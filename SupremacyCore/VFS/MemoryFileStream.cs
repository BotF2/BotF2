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

#region Using Statements

using System;
using System.IO;
using System.IO.Compression;

#endregion

namespace Supremacy.VFS
{
    public class MemoryFileStream : StreamDecorator<Stream>
    {
        #region Fields and Properties

        private MemorySource _parent;
        private CompressionAlgorithm _compression;

        #endregion

        #region Constructor

        // ReSharper disable SuggestBaseTypeForParameter
        public MemoryFileStream(
            MemorySource parent,
            string resolvedPath,
            MemoryStream stream,
            FileAccess access,
            FileShare share)
            // ReSharper restore SuggestBaseTypeForParameter
            : base(resolvedPath, stream, access, share)
        {
            _parent = parent;
        }

        #endregion

        #region Methods

        protected override void OnClosed()
        {
            _parent.UpdateFileBuffer(this);
            base.OnClosed();
        }

        internal void SetCompression(CompressionAlgorithm compression, CompressionMode mode)
        {
            _compression = compression;

            switch (_compression)
            {
                case CompressionAlgorithm.None:
                    break;

                case CompressionAlgorithm.GZip:
                    BaseStream = new GZipStream(BaseStream, mode);
                    break;

                case CompressionAlgorithm.Deflate:
                    BaseStream = new DeflateStream(BaseStream, mode);
                    break;

                default:
                    throw new NotSupportedException("Not supported compression algorithm used");
            }
        }

        internal byte[] GetBuffer()
        {
            switch (_compression)
            {
                case CompressionAlgorithm.None:
                    return ((MemoryStream)BaseStream).GetBuffer();

                case CompressionAlgorithm.GZip:
                    return ((MemoryStream)((GZipStream)BaseStream).BaseStream).GetBuffer();

                case CompressionAlgorithm.Deflate:
                    return ((MemoryStream)((DeflateStream)BaseStream).BaseStream).GetBuffer();

                default:
                    return null;
            }
        }

        #endregion
    }
}
