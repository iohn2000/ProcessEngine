using System;

namespace Kapsch.IS.ProcessEngine.Shared.Exceptions
{
    public class PermissionException : Exception
    {
        public bool IsCheckedOutByAnotherUser { get; set; }

        public bool IsNotCheckedOut { get; set; }
    }
}
