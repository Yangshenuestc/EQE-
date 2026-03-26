using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WPFAddDemo.ViewModels;

namespace WPFEQEDemo.ViewModels
{
    internal class InfoViewModel : NotifyBase
    {
		private StringBuilder info;

		public StringBuilder Info
		{
			get { return info; }
			set
			{
				info = value;
				this.RaisePropertyChanged("Info");
			}
		}

        private string presentedInfo;

        public string PresentedInfo
        {
            get { return presentedInfo; }
            set
            {
                presentedInfo = value;
                this.RaisePropertyChanged("PresentedInfo");
            }
        }

        public InfoViewModel()
		{
			this.Info = new StringBuilder();
		}
	}
}