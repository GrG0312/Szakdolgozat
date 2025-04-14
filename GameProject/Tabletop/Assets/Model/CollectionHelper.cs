using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public static class CollectionHelper
    {
        public static bool LoopbackSearch<T>(IList<T> collection, Predicate<T> condition, int startIndex, out int foundIndex)
        {
            foundIndex = default;

            // First search until the end of the collection
            for (int i = startIndex + 1; i < collection.Count; i++)
            {
                if (condition(collection[i]))
                {
                    foundIndex = i;
                    return true;
                }
            }

            // Then search until startIndex - 1
            for (int i = 0; i < startIndex; i++)
            {
                if (condition(collection[i]))
                {
                    foundIndex = i;
                    return true;
                }
            }

            // If we didnt find return false
            return false;
        }
    }
}
