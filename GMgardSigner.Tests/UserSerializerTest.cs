using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GMgardSigner.Tests
{
    [TestClass]
    public class UserSerializerTest
    {
        [TestMethod]
        public void ToBytesTest()
        {
            var user = User.Create("user", "pass");

            var bytes = user.ToBytes();

            Assert.AreEqual(true, bytes.Length > 0);
        }

        [TestMethod]
        public void ToUserTest()
        {
            var uri = new Uri("test://testdomain/");
            var user = User.Create("user", "pass");
            user.Cookies.Add(uri, new System.Net.Cookie("name", "value", "/"));
            var cookie = user.Cookies.GetCookies(uri)[0];

            var bytes = user.ToBytes();
            user = bytes.ToUser();
            var cookieActual = user.Cookies.GetCookies(uri)[0];

            Assert.AreEqual("user", user.Username);
            Assert.AreEqual("pass", user.Password);
            Assert.AreEqual(cookie, cookieActual);
        }
    }
}
