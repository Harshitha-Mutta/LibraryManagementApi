using System;

namespace LibraryManagementApi.Exceptions
{
    public class BorrowedBookException : Exception
    {
        public BorrowedBookException() { }

        public BorrowedBookException(string message)
            : base(message) { }

        public BorrowedBookException(string message, Exception inner)
            : base(message, inner) { }
    }
}
