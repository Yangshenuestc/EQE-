using System.Collections.Generic;
using WPFAddDemo.ViewModels;
using WPFEQEDemo.Models;

namespace WPFEQEDemo.ViewModels
{
    internal class PixelViewModel : NotifyBase
    {
        private List<Pixel> pixelList;

        public List<Pixel> PixelList
        {
            get { return pixelList; }
            set
            {
                pixelList = value;
                this.RaisePropertyChanged("PixelList");
            }
        }

        private ushort selectedPixelID;

        public ushort SelectedPixelID
        {
            get { return selectedPixelID; }
            set
            {
                selectedPixelID = value;
                this.RaisePropertyChanged("SelectedPixelID");
            }
        }

        public PixelViewModel()
        {
            this.PixelList = new List<Pixel>();
        }

    }
}
