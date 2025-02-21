using System.Collections.Generic;
using System.Linq;

namespace Model
{
    /// <summary>
    /// LIFO structured collection. When reaching a number of elements, specified by <see cref="Limit"/>, it will drop the 'oldest' element in the collection.
    /// </summary>
    /// <typeparam name="T">The type of the stored objects</typeparam>
    public class History<T>
    {
        public const int HISTORY_SIZE_LIMIT = 5;
        private readonly List<T> hidden;
        public int Limit { get; }

        public History(int limit = HISTORY_SIZE_LIMIT)
        {
            Limit = limit;
            hidden = new List<T>(Limit + 1);
        }

        public void Push(T element)
        {
            hidden.Add(element);
            while (hidden.Count > Limit)
            {
                hidden.RemoveAt(0);
            }
        }
        public T Pop()
        {
            T retval = hidden.ElementAt(hidden.Count - 1);
            hidden.RemoveAt(hidden.Count - 1);
            return retval;
        }
        public T Peek()
        {
            return hidden[hidden.Count - 1];
        }
        public bool Contains(T element)
        {
            return hidden.Contains(element);
        }
    }
}
