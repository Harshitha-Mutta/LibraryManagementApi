using System;

namespace LibraryManagementApi.Exceptions
{
    public class FineException : Exception
    {
        public FineException() { }

        public FineException(string message)
            : base(message) { }

        public FineException(string message, Exception inner)
            : base(message, inner) { }
    }
}
