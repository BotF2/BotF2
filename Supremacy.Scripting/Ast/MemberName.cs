using Microsoft.Scripting;

namespace Supremacy.Scripting.Ast
{
    public class MemberName
    {
        public static readonly MemberName Null = new MemberName("");

        private MemberName(MemberName left, string name, bool isDoubleColon, SourceSpan span)
        {
            Name = name;
            Span = span;
            IsDoubleColon = isDoubleColon;
            Left = left;
        }

        private MemberName(MemberName left, string name, bool isDoubleColon,
                    TypeArguments args, SourceSpan loc)
            : this(left, name, isDoubleColon, loc)
        {
            if (args != null && args.Count > 0)
                TypeArguments = args;
        }

        public MemberName() : this(null) { }

        public MemberName(string name)
            : this(name, SourceSpan.None)
        { }

        public MemberName(string name, SourceSpan loc)
            : this(null, name, false, loc)
        { }

        public MemberName(string name, TypeArguments args, SourceSpan loc)
            : this(null, name, false, args, loc)
        { }

        public MemberName(MemberName left, string name)
            : this(left, name, left != null ? left.Span : SourceSpan.None)
        { }

        public MemberName(MemberName left, string name, SourceSpan loc)
            : this(left, name, false, loc)
        { }

        public MemberName(MemberName left, string name, TypeArguments args, SourceSpan loc)
            : this(left, name, false, args, loc)
        { }

        public MemberName(string alias, string name, TypeArguments args, SourceSpan loc)
            : this(new MemberName(alias, loc), name, true, args, loc)
        { }

        public MemberName(MemberName left, MemberName right)
            : this(left, right, right.Span)
        { }

        public MemberName(MemberName left, MemberName right, SourceSpan loc)
            : this(null, right.Name, false, right.TypeArguments, loc)
        {
            if (right.IsDoubleColon)
                throw new SyntaxErrorException("Cannot append double_colon member name");
            Left = (right.Left == null) ? left : new MemberName(left, right.Left);
        }

        // TODO: Remove
        public string GetName()
        {
            return GetName(false);
        }

        public bool IsGeneric
        {
            get
            {
                if (TypeArguments != null)
                    return true;
                if (Left != null)
                    return Left.IsGeneric;
                return false;
            }
        }

        public string GetName(bool isGeneric)
        {
            string name = isGeneric ? BaseName : Name;
            if (Left != null)
                return Left.GetName(isGeneric) + (IsDoubleColon ? "::" : ".") + name;

            return name;
        }

        public TypeNameExpression GetTypeExpression()
        {
            if (Left == null)
                return new TypeNameExpression(BaseName, TypeArguments, Span);

            if (IsDoubleColon)
            {
                if (Left.Left != null)
                    throw new SyntaxErrorException("The left side of a :: should be an identifier");

                var qualifiedAliasMember = new QualifiedAliasMember
                                           {
                                               Alias = Left.Name,
                                               Name = Name,
                                               Span = Span
                                           };

                qualifiedAliasMember.TypeArguments.Add(TypeArguments);

                return qualifiedAliasMember;
            }

            Expression lexpr = Left.GetTypeExpression();
            
            var memberAccessExpression = new MemberAccessExpression
                   {
                       Name = Name,
                       Span = Span,
                       Left = lexpr
                   };
            
            memberAccessExpression.TypeArguments.Add(TypeArguments);

            return memberAccessExpression;
        }

        public string GetSignatureForError()
        {
            var append = (TypeArguments == null) ? string.Empty : "<" + TypeArguments.GetSignatureForError() + ">";
            if (Left == null)
                return Name + append;
            var connect = IsDoubleColon ? "::" : ".";
            return Left.GetSignatureForError() + connect + Name + append;
        }

        public MemberName Clone()
        {
            var leftClone = Left == null ? null : Left.Clone();
            return new MemberName(leftClone, Name, IsDoubleColon, TypeArguments, Span);
        }

        public string BaseName
        {
            get
            {
                if (TypeArguments != null)
                    return MakeName(Name, TypeArguments);
                return Name;
            }
        }

        public override bool Equals(object other)
        {
            return Equals(other as MemberName);
        }

        public bool Equals(MemberName other)
        {
            if (this == other)
                return true;
            if (other == null || Name != other.Name)
                return false;
            if (IsDoubleColon != other.IsDoubleColon)
                return false;

            if ((TypeArguments != null) &&
                (other.TypeArguments == null || TypeArguments.Count != other.TypeArguments.Count))
                return false;

            if ((TypeArguments == null) && (other.TypeArguments != null))
                return false;

            if (Left == null)
                return other.Left == null;

            return Left.Equals(other.Left);
        }

        public override int GetHashCode()
        {
            int hash = Name.GetHashCode();
            for (MemberName n = Left; n != null; n = n.Left)
                hash ^= n.Name.GetHashCode();
            if (IsDoubleColon)
                hash ^= 0xbadc01d;

            if (TypeArguments != null)
                hash ^= TypeArguments.Count << 5;

            return hash & 0x7FFFFFFF;
        }

        public int CountTypeArguments
        {
            get
            {
                if (TypeArguments != null)
                    return TypeArguments.Count;
                if (Left != null)
                    return Left.CountTypeArguments;
                return 0;
            }
        }

        public string Name { get; set; }

        public bool IsDoubleColon { get; set; }

        public TypeArguments TypeArguments { get; set; }

        public MemberName Left { get; set; }

        public SourceSpan Span { get; set; }

        public static string MakeName(string name, TypeArguments args)
        {
            if (args == null || args.Count == 0)
                return name;

            return name + "`" + args.Count;
        }

        public static string MakeName(string name, int count)
        {
            return name + "`" + count;
        }
    }
}