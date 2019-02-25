using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace ElFinder.Connector
{
    public class DefaultElFinderFactory : IElFinderFactory
    {
        public IOptions<DefaultElFinderFactoryOptions> Options { get; }
        public Image.IImageProcessor ImageProcessor { get; }

        public DefaultElFinderFactory(IOptions<DefaultElFinderFactoryOptions> options, Image.IImageProcessor imageProcessor)
        {
            Options = options;
            ImageProcessor = imageProcessor;
        }

        private static object _sync = new object();

        private ElFinder _elFinder;

        public ElFinder Create()
        {
            if (_elFinder == null)
            {
                lock (_sync)
                {
                    if (_elFinder == null)
                    {
                        _elFinder = new ElFinder
                        {
                            Volumes = Options.Value.Volumes,
                            ImageProcessor = ImageProcessor
                        };
                    }
                }
            }
            return _elFinder;
        }
    }
}
