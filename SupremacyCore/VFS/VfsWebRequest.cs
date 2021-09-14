// VfsWebRequest.cs
// 
// Copyright (c) 2008 Mike Strobel
// 
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
// 
// All other rights reserved.

using System;
using System.Net;

using Supremacy.Annotations;

using System.IO;

using Supremacy.Resources;

namespace Supremacy.VFS
{
    internal class VfsWebRequest : WebRequest
    {
        private readonly Uri _requestUri;

        public override Uri RequestUri => _requestUri;

        public VfsWebRequest([NotNull] Uri requestUri)
        {
            _requestUri = requestUri ?? throw new ArgumentNullException("requestUri");
        }

        public override WebResponse GetResponse()
        {
            return new VfsWebResponse(RequestUri);
        }
    }

    public class VfsWebRequestFactory : IWebRequestCreate
    {
        public const string Scheme = VfsService.UriScheme;

        public static void EnsureRegistered()
        {
            lock (typeof(VfsWebRequestFactory))
            {
                if (UriParser.IsKnownScheme("vfs"))
                {
                    return;
                }

                UriParser.Register(
                    new GenericUriParser(
                        GenericUriParserOptions.AllowEmptyAuthority |
                        GenericUriParserOptions.GenericAuthority |
                        GenericUriParserOptions.NoPort |
                        GenericUriParserOptions.NoQuery |
                        GenericUriParserOptions.NoUserInfo),
                    Scheme,
                    -1);

                _ = WebRequest.RegisterPrefix(Scheme, new VfsWebRequestFactory());
            }
        }

        #region Implementation of IWebRequestCreate
        public WebRequest Create(Uri uri)
        {
            return new VfsWebRequest(uri);
        }
        #endregion
    }

    internal class VfsWebResponse : WebResponse
    {
        private readonly Uri _responseUri;

        public sealed override Uri ResponseUri => _responseUri;

        public VfsWebResponse([NotNull] Uri responseUri)
        {
            _responseUri = responseUri ?? throw new ArgumentNullException("responseUri");
        }

        private readonly Lazy<IVfsService> _vfsService = new Lazy<IVfsService>(() => ResourceManager.VfsService);

        protected IVfsService VfsService => _vfsService.Value;

        public override Stream GetResponseStream()
        {
            IVfsService vfsService = VfsService;
            if (vfsService == null)
            {
                throw new InvalidOperationException("Could not resolve VFS service.");
            }


            if (!vfsService.TryGetFileInfo(_responseUri, out IVirtualFileInfo virtualFileInfo))
            {
                throw new FileNotFoundException(
                    string.Format(
                        "Could not locate virtual file '{0}'.",
                        _responseUri));
            }

            return virtualFileInfo.OpenRead();
        }
    }
}