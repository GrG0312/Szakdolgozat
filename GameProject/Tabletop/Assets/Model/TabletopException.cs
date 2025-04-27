using System;

namespace Model
{
    public class TabletopException : Exception
    {
        public TabletopException() { }
        public TabletopException(string message) : base(message) { }
    }
}
