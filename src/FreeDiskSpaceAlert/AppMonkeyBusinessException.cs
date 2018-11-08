using System;

namespace FreeDiskSpaceAlert
{
    public class AppMonkeyBusinessException : Exception
    {
        public AppMonkeyBusinessException(string message) : base(message){}
    }
}