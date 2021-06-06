using System;
using System.Globalization;

using Supremacy.Annotations;

namespace Supremacy.Types
{
    [Serializable]
    public sealed class NamedGuid : IEquatable<NamedGuid>
    {
        private readonly Guid _guid;
        private readonly string _name;

        public NamedGuid(Guid guid, [NotNull] string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name", "Value must be a non-null, non-empty string.");

            _guid = guid;
            _name = name;
        }

        public Guid Guid => _guid;

        public string Name => _name;

        public override string ToString()
        {
            if (_name[0] != '{')
                return string.Format(CultureInfo.InvariantCulture, "{{{0}}}", _name);

            return _name;
        }

        public bool Equals(NamedGuid other)
        {
            if (ReferenceEquals(null, other))
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return other._guid.Equals(_guid);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as NamedGuid);
        }

        public override int GetHashCode()
        {
            return _guid.GetHashCode();
        }

        public static bool operator ==(NamedGuid left, NamedGuid right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(NamedGuid left, NamedGuid right)
        {
            return !Equals(left, right);
        }
    }
}