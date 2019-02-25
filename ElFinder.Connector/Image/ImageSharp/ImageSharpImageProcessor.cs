using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ElFinder.Connector.Image.ImageSharp
{
    public class ImageSharpImageProcessor : IImageProcessor
    {
        public IImg Load(Stream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);
            return new ImageSharpImage<SixLabors.ImageSharp.PixelFormats.Rgba32>(SixLabors.ImageSharp.Image.Load(stream));
        }
    }
}
