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
using System.Collections.Generic;
using System.Globalization;





#endregion

namespace Supremacy.VFS.Utilities
{
    /// <summary>
    /// Class that performs string comparisons taking into account case and culture.
    /// </summary>
    public class CaseCultureStringComparer : IComparer<string>, IEqualityComparer<string>
    {
        #region Fields and Properties

        private bool _isCaseSensitive;
        /// <summary>
        /// Gets or sets a value indicating whether this instance uses case sensitive comparisons.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance uses case sensitive comparisons; otherwise, <c>false</c>.
        /// </value>
        public bool IsCaseSensitive
        {
            get { return _isCaseSensitive; }
            set { _isCaseSensitive = value; }
        }

        private bool _isInvariantCulture;
        /// <summary>
        /// Gets or sets a value indicating whether this instance uses invariant culture comparisons.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance uses invariant culture comparisons; otherwise, <c>false</c>.
        /// </value>
        public bool IsInvariantCulture
        {
            get { return _isInvariantCulture; }
            set
            {
                if (_isInvariantCulture == value)
                    return;

                _isInvariantCulture = value;

                if (_isInvariantCulture)
                    _cultureInfo = CultureInfo.InvariantCulture;
                else
                    _cultureInfo = new CultureInfo(_cultureName);
            }
        }

        private CultureInfo _cultureInfo;
        /// <summary>
        /// Gets the current culture.
        /// </summary>
        /// <value>The current culture.</value>
        public CultureInfo CultureInfo => _cultureInfo;

        /// <summary>
        /// Name of the current culture.
        /// </summary>
        private string _cultureName;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CaseCultureStringComparer"/> class.
        /// </summary>
        /// <param name="isCaseSensitive">If set to <c>true</c> the equality is case sensitive.</param>
        /// <param name="isInvariantCulture">If set to <c>true</c> the culture is the invariant culture.</param>
        /// <param name="culture">The name of the culture to use for culture-based comparisons.</param>
        public CaseCultureStringComparer(bool isCaseSensitive, bool isInvariantCulture, string culture)
        {
            _isCaseSensitive = isCaseSensitive;
            _isInvariantCulture = isInvariantCulture;
            _cultureName = culture;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Sets the culture name.
        /// </summary>
        /// <param name="cultureName">Name of the culture.</param>
        public void SetCultureName(string cultureName)
        {
            _cultureName = cultureName;

            if (!_isInvariantCulture)
                _cultureInfo = new CultureInfo(_cultureName);
        }

        #endregion

        #region IEqualityComparer<string> Members

        /// <summary>
        /// Equalses the specified x.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <returns></returns>
        public bool Equals(string x, string y)
        {
            if (_isCaseSensitive && _isInvariantCulture)
                return x.Equals(y, StringComparison.InvariantCulture);

            if (!_isCaseSensitive && _isInvariantCulture)
                return x.Equals(y, StringComparison.InvariantCultureIgnoreCase);

            if (_isCaseSensitive && !_isInvariantCulture)
                return x.Equals(y, StringComparison.CurrentCulture);

            if (!_isCaseSensitive && !_isInvariantCulture)
                return x.Equals(y, StringComparison.CurrentCultureIgnoreCase);

            return false;
        }

        /// <summary>
        /// Returns a hash code for the specified object.
        /// </summary>
        /// <param name="obj">The <see cref="T:System.Object"/> for which a hash code is to be returned.</param>
        /// <returns>A hash code for the specified object.</returns>
        public int GetHashCode(string obj)
        {
            if (_isCaseSensitive)
                return obj.GetHashCode();

            if (_isInvariantCulture)
                return obj.ToLowerInvariant().GetHashCode();

            return obj.ToLower().GetHashCode();
        }

        #endregion

        #region IComparer<string> Members

        /// <summary>
        /// Compares the specified x.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <returns></returns>
        public int Compare(string x, string y)
        {
            if (_isCaseSensitive)
                return _cultureInfo.CompareInfo.Compare(x, y, CompareOptions.None);

            return _cultureInfo.CompareInfo.Compare(x, y, CompareOptions.IgnoreCase);
        }
        #endregion
    }
}
