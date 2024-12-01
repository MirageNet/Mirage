using System;

namespace Mirage.RemoteCalls
{
    public class ReturnRpcException : Exception
    {
        public ReturnRpcException(string message) : base(message) { }
    }
}
