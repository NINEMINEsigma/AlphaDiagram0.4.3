using System;
using System.Collections.Generic;
using System.Linq;
using AD.BASE;

namespace AD.Utility
{
    public static class ContainerExtension 
    {
        public static List<P> GetSubList<T,P>(this List<T> self) where P:class
        {
            List<P> result = new();
            result.AddRange(from T item in self
                            where item.Convertible<P>()
                            select item as P);
            return result;
        }

        public static P SelectCast<T,P>(this List<T> self) where P : class
        {
            foreach (var item in self)
                if (item.As<P>(out var result)) return result;
            return null;
        }

        public static List<T> GetSubList<T>(this IEnumerable<T> self,Predicate<T> predicate)
        { 
            List<T> result = new();
            result.AddRange(from T item in self
                            where predicate(item)
                            select item);
            return result;
        }

    }
}
