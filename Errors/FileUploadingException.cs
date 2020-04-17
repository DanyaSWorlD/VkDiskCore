using System;
using System.Runtime.Serialization;

namespace VkDiskCore.Errors
{
    [Serializable]
    public class FileUploadingException : Exception
    {
        public FileUploadingException()
            : base() { }

        public FileUploadingException(string message)
            : base(message) { }

        public FileUploadingException(string format, params object[] args)
            : base(string.Format(format, args)) { }

        public FileUploadingException(string message, Exception innerException)
            : base(message, innerException) { }

        public FileUploadingException(string format, Exception innerException, params object[] args)
            : base(string.Format(format, args), innerException) { }

        protected FileUploadingException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}
