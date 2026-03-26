using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HMTesterStation.EQE
{
    public class EQEDevInfo
    {
        public uint SiteID { get; set; }
        public string Model { get; set; }
        public string Firmware { get; set; }
        public EQEDevInfo() { }

    }
}
