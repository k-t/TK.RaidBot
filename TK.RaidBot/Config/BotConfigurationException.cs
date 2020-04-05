using System;
using System.Runtime.Serialization;

namespace TK.RaidBot.Config
{
    [Serializable]
    public class BotConfigurationException : Exception
    {
        public BotConfigurationException(string message)
            : base(message)
        {
        }

        protected BotConfigurationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
