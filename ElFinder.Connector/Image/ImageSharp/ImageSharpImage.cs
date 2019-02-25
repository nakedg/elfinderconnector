using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace ElFinder.Connector.Image.ImageSharp
{
    public class ImageSharpImage<T>: IImg where T: struct, IPixel<T>
    {
        private SixLabors.ImageSharp.Image<T> _image;

        public ImageSharpImage(SixLabors.ImageSharp.Image<T> image)
        {
            _image = image;
        }

        public string GetDeminisation()
        {
            return $"{_image.Width}x{_image.Height}";
        }

        public Stream Resize(int width, int height)
        {
            System.IO.MemoryStream ms = new MemoryStream();
            _image.Mutate(x => x.Resize(width, height));
            _image.SaveAsPng(ms);
            return ms;
        }

        public void Dispose()
        {
            _image.Dispose();
        }

    }
}
