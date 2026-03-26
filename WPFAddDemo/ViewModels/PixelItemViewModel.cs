using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WPFAddDemo.ViewModels;
using WPFEQEDemo.Models;

namespace WPFEQEDemo.ViewModels
{
    internal class PixelItemViewModel : NotifyBase
    {
        public PixelItemViewModel()
        {
            
        }

        public PixelItemViewModel(Pixel pixel, bool isSeleted)
        {
            this.Pixel = pixel;
        }

        public Pixel Pixel { get; set; }
	}
}
