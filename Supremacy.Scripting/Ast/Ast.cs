using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

using Microsoft.Scripting;

using System.Linq;

using Microsoft.Scripting.Utils;

using Supremacy.Annotations;
using Supremacy.Scripting.Runtime;

namespace Supremacy.Scripting.Ast
{
    public class Ast : IAst, ISupportInitializeNotification
    {
        private static int _count;
        private SourceSpan _sourceSpan;

        public Ast()
        {
            _sourceSpan = SourceSpan.None;
            UniqueId = _count++;
        }

        protected Ast(Ast copySource)
        {
            _sourceSpan = SourceSpan.None;
            UniqueId = copySource.UniqueId;
            _sourceSpan = copySource._sourceSpan;
        }

        public static IList<T> Clone<T>(CloneContext cloneContext, IEnumerable<T> list) where T : class, IAst
        {
            if (list == null)
            {
                return null;
            }

            return list.Select(o => Clone(cloneContext, o)).ToList();
        }

        public static T Clone<T>(CloneContext cloneContext, T item) where T : class, IAst
        {
            if (item == null)
            {
                return null;
            }

            T clone = (T)Activator.CreateInstance(item.GetType(), BindingFlags.Instance | BindingFlags.NonPublic);
            item.CloneTo(cloneContext, clone);

            LinkParents.Link(clone);

            return clone;
        }

        public static T[] Clone<T>(CloneContext cloneContext, params T[] array) where T : class, IAst
        {
            return array?.Select(o => Clone(cloneContext, o)).ToArray();
        }

        public virtual void CloneTo<T>(CloneContext cloneContext, T target) where T : class, IAst
        {
            target.Span = Span;
            target.FileName = FileName;
        }

        public T GetEnclosingAst<T>() where T : class, IAst
        {
            IAst parentAst = ParentAst;
            while (!(parentAst is T) && (parentAst != null))
            {
                parentAst = parentAst.ParentAst;
            }

            return parentAst as T;
        }

        public static IAst LeastCommonAncestor(IAst u, IAst v)
        {
            List<IAst> list = new List<IAst>();
            while (u != null)
            {
                list.Add(u);
                u = u.ParentAst;
            }
            while (v != null)
            {
                if (list.Contains(v))
                {
                    return v;
                }

                v = v.ParentAst;
            }
            return null;
        }

        public virtual void Walk(AstVisitor prefix, AstVisitor postfix)
        {
            //throw new Exception(
            //    string.Format("Ast.Walk called; implement in derived class {0}.", 
            //                  base.GetType().FullName));
        }

        public static void Walk<T>(T[] array, AstVisitor prefix, AstVisitor postfix) where T : IAst
        {
            if (array == null)
            {
                return;
            }

            for (int i = 0; i < array.Length; i++)
            {
                T node = array[i];
                if (node == null)
                {
                    continue;
                }

                Walk(ref node, prefix, postfix);
                array[i] = node;
            }
        }

        public static void Walk<T>(ref T node, AstVisitor prefix, AstVisitor postfix) where T : IAst
        {
            IAst first = node;
            IAst second = node;

            if (first == null)
            {
                return;
            }

            if (prefix(ref first))
            {
                first.Walk(prefix, postfix);
            }

            if (second.Equals(first))
            {
                _ = postfix(ref first);
            }

            if (second.Equals(first))
            {
                return;
            }

            if (first is object)
            {
                if (!(first is T))
                {
                    throw new Exception(
                        string.Format(
                            "Node replaced not of the correct type {0}.",
                            typeof(T).Name));
                }
                LinkParents.Link(first);
                first.ParentAst = node.ParentAst;
            }

            node = (T)first;
        }

        public static void WalkList<T>(IList<T> nodeList, AstVisitor prefix, AstVisitor postfix) where T : IAst
        {
            if (nodeList == null)
            {
                return;
            }

            for (int i = 0; i < nodeList.Count; i++)
            {
                T node = nodeList[i];
                T currentNode = node;

                if (node == null)
                {
                    continue;
                }

                Walk(ref node, prefix, postfix);

                if (currentNode.Equals(node))
                {
                    continue;
                }

                nodeList.RemoveAt(i);

                if (node is object)
                {
                    nodeList.Insert(i, node);
                }
                else
                {
                    i--;
                }
            }
        }

        [DefaultValue(null)]
        public string FileName { get; set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IAst ParentAst { get; set; }

        public void Dump(SourceWriter sb)
        {
            Dump(sb, 0);
        }

        public virtual void Dump(SourceWriter sw, int indentChange) { }

        public virtual void BeginInit(ParseContext parseContext, bool raiseInitialized)
        {
            BeginInit();

            if (raiseInitialized)
            {
                OnInitialized();
            }
        }

        public virtual void EndInit(ParseContext parseContext)
        {
            EndInit();
        }

        [TypeConverter(typeof(SourceSpanConverter))]
        public virtual SourceSpan Span
        {
            get => _sourceSpan;
            set => _sourceSpan = value;
        }

        public int UniqueId { get; }

        protected static void DumpChild(IAst child, SourceWriter sw, int indentChange = 0)
        {
            if (child != null)
            {
                child.Dump(sw, indentChange);
            }
        }

        public void BeginInit()
        {
            IsInitialized = false;
        }

        public void EndInit()
        {
            IsInitialized = true;
        }

        public bool IsInitialized { get; private set; }

        public event EventHandler Initialized;

        protected void OnInitialized()
        {
            if (IsInitialized)
            {
                return;
            }

            Initialized?.Invoke(this, EventArgs.Empty);
        }
    }

    public static class CloningExtensions
    {
        public static void CloneTo<T>([NotNull] this IEnumerable<T> source, [NotNull] CloneContext cloneContext, [NotNull] IList<T> targetList) where T : class, IAst
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (cloneContext == null)
            {
                throw new ArgumentNullException("cloneContext");
            }

            if (targetList == null)
            {
                throw new ArgumentNullException("targetList");
            }

            targetList.AddRange(source.Select(item => Ast.Clone(cloneContext, item)));
        }
    }
}