using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;

namespace GMgardSigner
{
    public static class UserSerializer
    {
        public static byte[] ToBytes(this User user)
        {
            using (var ms = new MemoryStream())
            {
                var bf = new BinaryFormatter();
                bf.Serialize(ms, user.Username);
                bf.Serialize(ms, user.Password);
                bf.Serialize(ms, user.Cookies);
                var bytes = ProtectedData.Protect(ms.ToArray(), null, DataProtectionScope.CurrentUser);
                return bytes;
            }
        }

        public static User ToUser(this byte[] bytes)
        {
            bytes = ProtectedData.Unprotect(bytes, null, DataProtectionScope.CurrentUser);
            using (var ms = new MemoryStream(bytes))
            {
                var bf = new BinaryFormatter();
                var username = bf.Deserialize(ms).ToString();
                var password = bf.Deserialize(ms).ToString();
                var cookies = bf.Deserialize(ms) as System.Net.CookieContainer;
                var user = User.Create(username, password, cookies);

                return user;
            }
        }
    }
}
