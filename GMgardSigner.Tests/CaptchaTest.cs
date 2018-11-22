using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Drawing;
using System.IO;

namespace GMgardSigner.Tests
{
    [TestClass]
    public class CaptchaTest
    {
        [DataTestMethod]
        #region  [10 Captcha Files]
        [DataRow("SharedFiles/Captcha/18-3.jpg")]
        [DataRow("SharedFiles/Captcha/19+0.jpg")]
        [DataRow("SharedFiles/Captcha/37-0.jpg")]
        [DataRow("SharedFiles/Captcha/47-0.jpg")]
        [DataRow("SharedFiles/Captcha/61+4.jpg")]
        [DataRow("SharedFiles/Captcha/68-7.jpg")]
        [DataRow("SharedFiles/Captcha/71-5.jpg")]
        [DataRow("SharedFiles/Captcha/77+3.jpg")]
        [DataRow("SharedFiles/Captcha/92+7.jpg")]
        [DataRow("SharedFiles/Captcha/93+3.jpg")]
        #endregion
        public void ReadTest(string fileName)
        {
            var image = new Bitmap(fileName);
            var expected = Path.GetFileNameWithoutExtension(fileName);

            Captcha.Read(image, out var actual);

            Assert.AreEqual(expected, actual);
        }

        [DataTestMethod]
        #region  [2 Captcha Files]
        [DataRow("SharedFiles/Captcha/61+4.jpg", 65)]
        [DataRow("SharedFiles/Captcha/68-7.jpg", 61)]
        #endregion
        public void ReadResultTest(string fileName, int expected)
        {
            var image = new Bitmap(fileName);

            var actual = Captcha.Read(image, out _);

            Assert.AreEqual(expected, actual);
        }
    }
}
