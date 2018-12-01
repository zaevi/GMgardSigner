using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace GMgardSigner.Tests
{

    [TestClass]
    public class UserTest
    {
        static User testUser;
        static User TestUser { get {
                if(testUser == null)
                {
                    var username = Environment.GetEnvironmentVariable("GMgardSignerTestUser");
                    var pass = Environment.GetEnvironmentVariable("GMgardSignerTestPass");

                    Assert.IsNotNull(username, @"Please set environment variable GMgardSignerTestUser and GMgardSignerTestPass to test");
                    Assert.IsNotNull(pass, @"Please set environment variable GMgardSignerTestUser and GMgardSignerTestPass to test");

                    testUser = User.Create(username, pass);
                }
                return testUser;
            } }

        [TestMethod]
        public async Task CheckLoginTest()
        {
            var user = User.Create("xxxx", "xxxx");

            var result = await user.CheckLoginAsync();

            Assert.AreEqual(false, result);
        }

        [TestMethod]
        public async Task LoginErrorTest()
        {
            var user = User.Create("test", "test");

            try
            {
                await user.LoginAsync();
            }
            catch { return; }
            Assert.Fail("login not failed");
        }

        [TestMethod]
        public async Task LoginTest()
        {
            await TestUser.LoginAsync();
        }

        [TestMethod]
        public async Task SignInTest()
        {
            if(!(await TestUser.CheckLoginAsync()))
                await TestUser.LoginAsync();

            var result = await TestUser.SignInAsync();

            Assert.AreEqual(true, result.Days > 0);
        }
    }
}
