using System;

namespace Actor.Core
{
    public class FirewallHelperException : Exception
    {
        public FirewallHelperException(string message)
            : base(message)
        { }
    }
}