using System;

namespace LibraryManagementApi.Exceptions
{
    public class GenreException : Exception
    {
        public GenreException() { }

        public GenreException(string message)
            : base(message) { }

        public GenreException(string message, Exception inner)
            : base(message, inner) { }
    }
}
