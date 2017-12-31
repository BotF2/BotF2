using System.Collections.Generic;

namespace Supremacy.Scripting.Ast
{
    public class UsingDirectivesExpression : Expression
    {
        private readonly List<UsingEntry> _entries = new List<UsingEntry>();

        public IList<UsingEntry> Entries
        {
            get { return _entries; }
        }

        public override Expression DoResolve(Runtime.ParseContext parseContext)
        {
            foreach (var entry in _entries)
                parseContext.AddUsing(entry);
            return null;
        }

        public override void Dump(Runtime.SourceWriter sw, int indentChange)
        {
            foreach (var entry in _entries)
                sw.WriteLine("using {0}", entry);
        }
    }
}