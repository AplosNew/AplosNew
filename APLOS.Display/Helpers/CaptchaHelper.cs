using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Web;

namespace Aplos.Helpers
{
    public class CaptchaHelper
    {
        private const string SessionKey = "__imageSessionKey_";
        private string GetRandomString()
        {

            string returnString = string.Empty;
            string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

            Random rand = new Random();

            int length = rand.Next(5, 8);
            for (int i = 0; i < length; i++)
            {
                int pos = rand.Next(0, 62);
                returnString += letters[pos].ToString();
            }
            return returnString;
        }
        public byte[] DrawByte()
        {
            byte[] returnByte;
            Bitmap bitmapImage = new Bitmap(150, 30, PixelFormat.Format32bppArgb);

            // Here we generate random string
            string key = GetRandomString();

            // key string adding to Session
            HttpContext.Current.Session.Add(SessionKey, key);

            // Creating image with key
            using (Graphics g = Graphics.FromImage(bitmapImage))
            {
                g.SmoothingMode = SmoothingMode.HighQuality;
                var rect = new Rectangle(0, 0, 150, 30);
                HatchBrush hBrush = new HatchBrush(HatchStyle.SmallConfetti, Color.LightGray, Color.White);
                g.FillRectangle(hBrush, rect);
                hBrush = new HatchBrush(HatchStyle.LargeConfetti, Color.Red, Color.Black);
                float fontSize = 20;
                Font font = new Font(FontFamily.GenericSerif, fontSize, FontStyle.Strikeout);
                float x = 10;
                float y = 1;
                PointF fPoint = new PointF(x, y);
                g.DrawString(key, font, hBrush, fPoint);

                using (MemoryStream ms = new MemoryStream())
                {
                    bitmapImage.Save(ms, ImageFormat.Jpeg);
                    returnByte = ms.ToArray();
                }
            }
            return returnByte;
        }
        public bool Verify(string key)
        {
            bool success = false;
            if (HttpContext.Current.Session[SessionKey] != null)
            {
                string sessionKey = HttpContext.Current.Session[SessionKey].ToString();
                if (sessionKey == key)
                {
                    success = true;
                }
            }
            return success;
        }
    }
}