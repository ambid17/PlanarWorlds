using System;
using System.Collections.Generic;
using System.Reflection;

namespace Digger.Modules.Core.Editor
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class OperationAttr : Attribute
    {
        private readonly string name;
        private readonly int order;

        public OperationAttr(string name, int order)
        {
            this.name = name;
            this.order = order;
        }

        public string Name => name;

        public int Order => order;

        public class Comparer : IComparer<Type>
        {
            public int Compare(Type a, Type b)
            {
                if (a == null && b == null)
                    return 0;
                if (a == null)
                    return 1;
                if (b == null)
                    return -1;

                var attrA = a.GetCustomAttribute(typeof(OperationAttr)) as OperationAttr;
                var attrB = b.GetCustomAttribute(typeof(OperationAttr)) as OperationAttr;

                if (attrA == null && attrB == null)
                    return string.CompareOrdinal(a.Name, b.Name);
                if (attrA == null)
                    return 1;
                if (attrB == null)
                    return -1;
                
                return attrA.Order.CompareTo(attrB.Order);
            }
        }
    }
}