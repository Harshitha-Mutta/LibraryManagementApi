using System;

namespace LibraryManagementApi.Exceptions
{
    public class EventException : Exception
    {
        public EventException() { }

        public EventException(string message)
            : base(message) { }

        public EventException(string message, Exception inner)
            : base(message, inner) { }
    }
}
