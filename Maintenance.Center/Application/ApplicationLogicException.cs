﻿using System.Runtime.Serialization;

namespace Application
{
    [Serializable]
    public class ApplicationLogicException : Exception
    {
        public ApplicationLogicException()
        {
        }

        public ApplicationLogicException(string message) : base(message)
        {
        }

        public ApplicationLogicException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ApplicationLogicException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
