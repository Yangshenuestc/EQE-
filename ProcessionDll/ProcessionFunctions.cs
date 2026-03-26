using HMTesterStation.EQE;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ProcessionDll
{
    public class ProcessionFunctions
    {
        private static object locker = new object();
        public ObservableCollection<string> DeviceList { get; private set; }

        public int SelectedEQEDevInd { get; set; }

        public ObservableCollection<EQEDevInfo> EQEDevList { get; private set; }

        public static ProcessionFunctions Instance { get; private set; }
        public ProcessionFunctions()
        {
            EQEDevList = new ObservableCollection<EQEDevInfo>();
            DeviceList = new ObservableCollection<string>();
        }

        public static ProcessionFunctions GetInstance()
        {
            //先检查Instance是否为null，防止每次调用都锁定locker，影响性能
            if (Instance == null)
            {
                lock (locker)
                {
                    if (Instance == null)
                    {
                        Instance = new ProcessionFunctions();
                    }
                }
            }
            return Instance;
        }

        public bool Initialization()
        {
            try
            {
                DeviceList = EQERobot.GetInstance().SearchDev();
            }
            catch (Exception)
            {
                return false;
            }
            ObservableCollection<EQEDevInfo> dev_list = EQERobot.GetInstance().OpenDev(SelectedEQEDevInd);

            EQEDevList.Clear();
            foreach (EQEDevInfo dev in dev_list)
            {
                EQEDevList.Add(dev);
            }
            if (EQEDevList.Count >= 2)
            {
                EQEDevInfo item = EQEDevList[0];
                uint site_id_x = item.SiteID;
                item = EQEDevList[1];
                uint site_id_y = item.SiteID;
                EQERobot.GetInstance().SetSiteID(site_id_x, site_id_y);
            }
            return true;
        }

        public bool Reset(ref string info)
        {
            if (!EQERobot.GetInstance().GoHome(ref info))
            {
                return false;
            }

            return true;
        }

        public void Move2Pos(int pos, ref string info)
        {
            EQERobot.GetInstance().Move2Pos(pos, ref info);
        }
    }
}
