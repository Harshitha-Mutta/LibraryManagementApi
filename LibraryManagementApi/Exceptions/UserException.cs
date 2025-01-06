using System;

namespace LibraryManagementApi.Exceptions
{
    public class UserException : Exception
    {
        public UserException() { }

        public UserException(string message)
            : base(message) { }

        public UserException(string message, Exception inner)
            : base(message, inner) { }
    }
}
