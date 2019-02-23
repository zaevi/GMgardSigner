using System;
using System.Runtime.Serialization;

namespace GMgardSigner
{
    [DataContract]
    public class SignInResult
    {
        [DataMember(Name = "success")]
        public bool Success { get; set; }

        [DataMember(Name = "days")]
        public int Days { get; set; }

        [DataMember(Name = "exp")]
        public int Exp { get; set; }
    }

    public class GMgardException : Exception
    {
        public override string Message { get; }

        public new object Data { get; }

        public GMgardException(string message = null, Exception inner = null, object data = null) : base(message, inner)
        {
            Message = $"{message}" + (inner != null ? $" ({inner.GetType().Name}: {inner.Message})" : string.Empty);
            Data = data;
        }
    }
}
