using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WPFEQEDemo.Models;

namespace WPFEQEDemo.Services
{
    internal class PixelItemInitService : IPixelItemService
    {
        public List<Pixel> InitPixel()
        {
            List<Pixel> pixels = new List<Pixel>();
            for (ushort i = 1; i <= 6;  i++)
            {
                pixels.Add(new Pixel(i));
            }
            return pixels;
        }
    }
}
