using System;

namespace FreeSpaceAlert
{
    public class AppMonkeyBusinessException : Exception
    {
        public AppMonkeyBusinessException(string message) : base(message){}
    }
}