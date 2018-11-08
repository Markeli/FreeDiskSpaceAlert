using System;

namespace Monitor
{
    public class AppMonkeyBusinessException : Exception
    {
        public AppMonkeyBusinessException(string message) : base(message){}
    }
}