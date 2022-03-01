using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using stdole;

namespace CnSharp.VisualStudio.Extensions.Resources
{
    public class ImageConverter : AxHost
    {
        public ImageConverter()
            : base("59EE46BA-677D-4d20-BF10-8D8067CB8B33")
        {
        }

        public static IPictureDisp ImageToIPicture(Image image)
        {
            var bmp = new Bitmap(image);
            MakeBackgroundAlphaZero(bmp);
            return (IPictureDisp) GetIPictureFromPicture(bmp);
        }

        private static void MakeBackgroundAlphaZero(Bitmap img)
        {
            var pixel = img.GetPixel(0, img.Height - 1);
            img.MakeTransparent(Color.FromArgb(255, 0, 255));
            var color = Color.FromArgb(0, pixel);
            img.SetPixel(0, img.Height - 1, color);
        }

        public static Image IPictureToImage(StdPicture picture)
        {
            return GetPictureFromIPicture(picture);
        }

        public static Bitmap MakeTransparentGif(Bitmap bitmap, Color color)
        {
            var R = color.R;
            var G = color.G;
            var B = color.B;

            var fin = new MemoryStream();
            bitmap.Save(fin, ImageFormat.Gif);

            var fout = new MemoryStream((int) fin.Length);
            var count = 0;
            var buf = new byte[256];
            byte transparentIdx = 0;
            fin.Seek(0, SeekOrigin.Begin);
            //header   
            count = fin.Read(buf, 0, 13);
            if ((buf[0] != 71) || (buf[1] != 73) || (buf[2] != 70)) return null; //GIF   

            fout.Write(buf, 0, 13);

            var i = 0;
            if ((buf[10] & 0x80) > 0)
            {
                i = 1 << ((buf[10] & 7) + 1) == 256 ? 256 : 0;
            }

            for (; i != 0; i--)
            {
                fin.Read(buf, 0, 3);
                if ((buf[0] == R) && (buf[1] == G) && (buf[2] == B))
                {
                    transparentIdx = (byte) (256 - i);
                }
                fout.Write(buf, 0, 3);
            }

            var gcePresent = false;
            while (true)
            {
                fin.Read(buf, 0, 1);
                fout.Write(buf, 0, 1);
                if (buf[0] != 0x21) break;
                fin.Read(buf, 0, 1);
                fout.Write(buf, 0, 1);
                gcePresent = (buf[0] == 0xf9);
                while (true)
                {
                    fin.Read(buf, 0, 1);
                    fout.Write(buf, 0, 1);
                    if (buf[0] == 0) break;
                    count = buf[0];
                    if (fin.Read(buf, 0, count) != count) return null;
                    if (gcePresent)
                    {
                        if (count == 4)
                        {
                            buf[0] |= 0x01;
                            buf[3] = transparentIdx;
                        }
                    }
                    fout.Write(buf, 0, count);
                }
            }
            while (count > 0)
            {
                count = fin.Read(buf, 0, 1);
                fout.Write(buf, 0, 1);
            }
            fin.Close();
            fout.Flush();

            return new Bitmap(fout);
        }
    }
}