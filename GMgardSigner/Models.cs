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
}
