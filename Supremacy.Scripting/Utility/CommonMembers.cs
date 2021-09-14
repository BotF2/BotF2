using System;
using System.Reflection;

namespace Supremacy.Scripting.Utility
{
    public static class CommonMembers
    {
        #region IDisposable.Dispose() Method

        private static MethodInfo _disposableDispose;

        public static MethodInfo DisposableDispose
        {
            get
            {
                if (_disposableDispose == null)
                {
                    _disposableDispose = typeof(IDisposable).GetMethod("Dispose");
                }

                return _disposableDispose;
            }
        }

        #endregion

        #region String.Format() Method

        private static MethodInfo _stringFormat;

        public static MethodInfo StringFormat
        {
            get
            {
                if (_stringFormat == null)
                {
                    _stringFormat = typeof(string).GetMethod(
                        "Format",
                        new[] { typeof(string), typeof(object).MakeArrayType() });
                }
                return _stringFormat;
            }
        }

        #endregion

        #region Object.ToString() Method

        private static MethodInfo _toString;

        public static MethodInfo ObjectToString
        {
            get
            {
                if (_toString == null)
                {
                    _toString = typeof(object).GetMethod("ToString");
                }

                return _toString;
            }
        }

        #endregion

        #region String.Concat(Object, Object) Method

        private static MethodInfo _stringConcat;

        public static MethodInfo StringConcat
        {
            get
            {
                if (_stringConcat == null)
                {
                    _stringConcat = TypeManager.CoreTypes.String.GetMethod("Concat", new[] { typeof(object).MakeArrayType() });
                }

                return _stringConcat;
            }
        }

        #endregion

    }
}