using System;
using System.Runtime.Serialization;

namespace TK.RaidBot.Services
{
    [Serializable]
    public class DataServiceException : Exception
    {
        public DataServiceException(string message)
            : base(message)
        {
        }

        protected DataServiceException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
