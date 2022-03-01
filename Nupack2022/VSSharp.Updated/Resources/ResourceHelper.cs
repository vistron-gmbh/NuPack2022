using System.Drawing;
using System.Resources;
using stdole;

namespace CnSharp.VisualStudio.Extensions.Resources
{
    public static class ResourceHelper
    {
        public static Bitmap LoadBitmap(this ResourceManager rm, string name)
        {
            var map = (Bitmap)rm.GetObject(name);
            return map;
        }

        public static StdPicture LoadPicture(this ResourceManager rm, string name)
        {
            var map = (Bitmap)rm.GetObject(name);
            return (StdPicture)ImageConverter.ImageToIPicture(map);
        }

    }
}
