using System;
using System.Collections;
using System.Drawing;
using System.Linq;

namespace GMgardSigner
{
    /// <summary>
    /// GMgard captcha reader
    /// </summary>
    public static class Captcha
    {
        static Rectangle[] CropRectangles = {
            new Rectangle(6, 8, 9, 15),     // A
            new Rectangle(17, 8, 9, 15),    // B
            new Rectangle(47, 8, 9, 15),    // C of AB-C
            new Rectangle(55, 8, 9, 15),    // C of AB+C
            new Rectangle(35, 11, 11, 12),  // +
        };

        public static int Read(Bitmap image, out string parsedText)
        {
            var a = CaptchaModel.ReadNumber(image.CropAndToBitArray(CropRectangles[0]));
            var b = CaptchaModel.ReadNumber(image.CropAndToBitArray(CropRectangles[1]));
            var flag = CaptchaModel.ReadPlus(image.CropAndToBitArray(CropRectangles[4]));
            var c = CaptchaModel.ReadNumber(image.CropAndToBitArray(flag ? CropRectangles[3] : CropRectangles[2]));

            parsedText = $"{a}{b}{(flag ? '+' : '-')}{c}";
            return a * 10 + b + (flag ? 1 : -1) * c;
        }

        public static BitArray CropAndToBitArray(this Bitmap image, Rectangle rect, float level=200/255f)
        {
            var size = rect.Width * rect.Height;
            var bitArray = new BitArray(size % 8 == 0 ? size : (size + 8 - size % 8));
            var i = 0;
            foreach(var y in Enumerable.Range(rect.Y, rect.Height))
                foreach(var x in Enumerable.Range(rect.X, rect.Width))
                    bitArray.Set(i++, image.GetPixel(x, y).GetBrightness() < level);
            
            return bitArray;
        }

    }

    public class CaptchaModel
    {

        #region [Fields] ...
        private static readonly string[] modelBase64 =
        {
            "fPwZOzx48ODBgwcPPmzMHx8=", // 0~9
            "MGD48AEDBgwYMGDAgAHDv38=",
            "fv4NAwYIGDAwcHBwcHDg/38=",
            "fv4FAw4MHB48wAADBh7snx8=",
            "4ODBw8bM2LBg//8DAwYMGDA=",
            "/v0bMGDAgB9/wAEDBj78nx8=",
            "8fg5MDBgz//jgwcPPmzcHx8=",
            "//8DBgwMGBgwMGBgwMCAgQM=",
            "/PwdHzz4uD9/wwcPHnzcHx8=",
            "fv4dGzx48ODD/nkDBgbOjwc=",
            "IAABCEAAg///HwYwAAEIQAA=", // + 
        };
        private static BitArray[] numberModel;
        private static BitArray plusModel;
        #endregion

        public static BitArray PlusModel
        {
            get
            {
                plusModel = plusModel ?? Base64ToBitArray(modelBase64[10]);
                return plusModel;
            }
        }
        public static BitArray[] NumberModel
        {
            get
            {
                numberModel = numberModel ?? modelBase64.Take(10).Select(b => Base64ToBitArray(b)).ToArray();
                return numberModel;
            }
        }

        private static BitArray Base64ToBitArray(string base64)
        {
            return new BitArray(Convert.FromBase64String(base64));
        }

        private static double GetDistanceRatio(BitArray model, BitArray instance)
        {
            var matched = 0;
            var length = model.Length;
            for (var i = 0; i < model.Length; i++)
                if (model[i] && instance[i]) matched++;

            return (double)matched / length;
        }

        private static double GetDistanceRatioEx(BitArray model, BitArray instance)
        {

            var matched = 0;
            var length = 0;
            for (var i = 0; i < model.Length; i++)
                if (model[i])
                {
                    length++;
                    if (instance[i]) matched++;
                }
            return (double)matched / length;
        }

        public static int ReadNumber(BitArray instance)
        {
            var ratios = NumberModel.Select(m => GetDistanceRatioEx(m, instance)).ToList();
            return ratios.IndexOf(ratios.Max());
        }

        public static bool ReadPlus(BitArray instance)
        {
            var ratio = GetDistanceRatioEx(PlusModel, instance);
            return ratio > 0.8;
        }
    }
}
