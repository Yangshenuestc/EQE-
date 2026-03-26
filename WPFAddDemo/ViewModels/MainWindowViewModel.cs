using HMTesterStation.EQE;
using ProcessionDll;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using WPFEQEDemo.Models;
using WPFEQEDemo.Properties;
using WPFEQEDemo.Services;
using WPFEQEDemo.ViewModels;

namespace WPFAddDemo.ViewModels
{
	internal class MainWindowViewModel : NotifyBase
	{
        public ObservableCollection<EQEDevInfo> EQEDevList { get; private set; }

        private int selectedEQEDevInd;

        public int SelectedEQEDevInd
        {
            get { return selectedEQEDevInd; }
            set
            {
                selectedEQEDevInd = value;
                this.RaisePropertyChanged("SelectedEQEDevInd");
            }
        }

        private ObservableCollection<string> deviceList;

        public ObservableCollection<string> DeviceList
        {
            get { return deviceList; }
            set
            {
                deviceList = value;
                this.RaisePropertyChanged("DeviceList");
            }
        }

        private PixelViewModel pixelViewModel;

        public PixelViewModel PixelViewModelInstance
        {
            get { return pixelViewModel; }
            set
            {
                pixelViewModel = value;
                this.RaisePropertyChanged("PixelViewModelInstance");
            }
        }

        private InfoViewModel infoViewModel;

        public InfoViewModel InfoViewModel
        {
            get { return infoViewModel; }
            set
            {
                infoViewModel = value;
                this.RaisePropertyChanged("InfoViewModel");
            }
        }

        // 1. 添加供界面绑定的 X 和 Y 属性
        public int UIBindingPulseX
        {
            get { return Settings.Default.PulseX_Pos1; }
            set
            {
                Settings.Default.PulseX_Pos1 = value;
                this.RaisePropertyChanged("UIBindingPulseX");
            }
        }

        public int UIBindingPulseY
        {
            get { return Settings.Default.PulseY_Pos1; }
            set
            {
                Settings.Default.PulseY_Pos1 = value;
                this.RaisePropertyChanged("UIBindingPulseY");
            }
        }

        // 2. 添加一个保存命令
        public CommandBase SaveSettingsCommand { get; set; }

        public CommandBase ResetCommand { get; set; }

        public CommandBase InitCommand { get; set; }

        public CommandBase Move21PosCommand { get; set; }

        public CommandBase Move22PosCommand { get; set; }

        public CommandBase Move23PosCommand { get; set; }

        public CommandBase Move24PosCommand { get; set; }

        public CommandBase Move25PosCommand { get; set; }

        public CommandBase Move26PosCommand { get; set; }

        public MainWindowViewModel()
        {
            this.infoViewModel = new InfoViewModel();
            //ScanPosCommand = new CommandBase
            //{
            //    ExcecuteAction = ScanPosCommandExecute
            //};
            ResetCommand = new CommandBase
            {
                ExcecuteAction = ResetCommandExecute
            };
            InitCommand = new CommandBase
            {
                ExcecuteAction = InitCommandExecute
            };
            //DisableCommand = new CommandBase
            //{
            //    ExcecuteAction = DisableCommandExecute
            //};
            Move21PosCommand = new CommandBase
            {
                ExcecuteAction = Move21PosCommandExecute
            };
            Move22PosCommand = new CommandBase
            {
                ExcecuteAction = Move22PosCommandExecute
            };
            Move23PosCommand = new CommandBase
            {
                ExcecuteAction = Move23PosCommandExecute
            };
            Move24PosCommand = new CommandBase
            {
                ExcecuteAction = Move24PosCommandExecute
            };
            Move25PosCommand = new CommandBase
            {
                ExcecuteAction = Move25PosCommandExecute
            };
            Move26PosCommand = new CommandBase
            {
                ExcecuteAction = Move26PosCommandExecute
            };
            // 3. 初始化保存命令
            SaveSettingsCommand = new CommandBase
            {
                ExcecuteAction = SaveSettingsExecute
            };

            // 4. 在软件启动（ViewModel初始化）时，将本地保存的参数同步给底层的 EQERobot
            EQERobot.GetInstance().PulseX_Pos1 = this.UIBindingPulseX;
            EQERobot.GetInstance().PulseY_Pos1 = this.UIBindingPulseY;

            this.EQEDevList = new ObservableCollection<EQEDevInfo>();
            this.PixelViewModelInstance = new PixelViewModel();

            EQERobot.GetInstance().OnUpdateUIMSiteIDEvent += UpdateUIMSiteExecute;

            LoadPixelMenu();
            InitCommandExecute(this);
            System.Threading.Thread.Sleep(100);
            ResetCommandExecute(this);
        }

        private void LoadPixelMenu()
        {
            PixelItemInitService ps = new PixelItemInitService();
            List<Pixel> pixels = ps.InitPixel();
            foreach (Pixel item in pixels)
            {
                this.PixelViewModelInstance.PixelList.Add(item);
            }
        }

        private void UpdateUIMSiteExecute(uint siteID)
        {
            
        }

        private void Move21PosCommandExecute(object parameter)
        {
            string info = string.Empty;
            ProcessionFunctions.GetInstance().Move2Pos(1, ref info);
            this.InfoViewModel.Info.AppendLine(info);
            this.InfoViewModel.PresentedInfo = this.InfoViewModel.Info.ToString();
        }

        private void Move22PosCommandExecute(object parameter)
        {
            string info = string.Empty;
            ProcessionFunctions.GetInstance().Move2Pos(2, ref info);
            this.InfoViewModel.Info.AppendLine(info);
            this.InfoViewModel.PresentedInfo = this.InfoViewModel.Info.ToString();
        }

        private void Move23PosCommandExecute(object parameter)
        {
            string info = string.Empty;
            ProcessionFunctions.GetInstance().Move2Pos(3, ref info);
            this.InfoViewModel.Info.AppendLine(info);
            this.InfoViewModel.PresentedInfo = this.InfoViewModel.Info.ToString();
        }

        private void Move24PosCommandExecute(object parameter)
        {
            string info = string.Empty;
            ProcessionFunctions.GetInstance().Move2Pos(4, ref info);
            this.InfoViewModel.Info.AppendLine(info);
            this.InfoViewModel.PresentedInfo = this.InfoViewModel.Info.ToString();
        }

        private void Move25PosCommandExecute(object parameter)
        {
            string info = string.Empty;
            ProcessionFunctions.GetInstance().Move2Pos(5, ref info);
      
            this.InfoViewModel.Info.AppendLine(info);
            this.InfoViewModel.PresentedInfo = this.InfoViewModel.Info.ToString();
        }

        private void Move26PosCommandExecute(object parameter)
        {
            string info = string.Empty;
            ProcessionFunctions.GetInstance().Move2Pos(6, ref info);
            this.InfoViewModel.Info.AppendLine(info);
            this.InfoViewModel.PresentedInfo = this.InfoViewModel.Info.ToString();
        }

        private void ResetCommandExecute(object parameter)
        {
            string info = string.Empty;
            if (ProcessionFunctions.GetInstance().Reset(ref info))
            {
                this.InfoViewModel.Info.AppendLine(info);
                this.InfoViewModel.PresentedInfo = this.InfoViewModel.Info.ToString();
            }
            else
            {
                this.InfoViewModel.Info.AppendLine("Reset failed!");
                this.InfoViewModel.PresentedInfo = this.InfoViewModel.Info.ToString();
            }
        }

        private void InitCommandExecute(object parameter)
        {
            if (ProcessionFunctions.GetInstance().Initialization())
            {
                this.InfoViewModel.Info.AppendLine("System is initialized successfully.");
                this.InfoViewModel.PresentedInfo = this.InfoViewModel.Info.ToString();
            }
            else
            {
                this.InfoViewModel.Info.AppendLine("System Initialization failed!");
                this.InfoViewModel.PresentedInfo = this.InfoViewModel.Info.ToString();
            }
        }
        //保存命令的具体执行逻辑
        private void SaveSettingsExecute(object parameter)
        {
            // 将当前界面的值永久保存到本地文件中
            Settings.Default.Save();

            // 同步更新到底层控制类中，确保接下来的运动立即生效
            EQERobot.GetInstance().PulseX_Pos1 = this.UIBindingPulseX;
            EQERobot.GetInstance().PulseY_Pos1 = this.UIBindingPulseY;

            this.InfoViewModel.Info.AppendLine("位置1参数已成功保存！");
            this.InfoViewModel.PresentedInfo = this.InfoViewModel.Info.ToString();
        }
    }
}
