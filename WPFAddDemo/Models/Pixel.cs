using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFEQEDemo.Models
{
    internal class Pixel
    {
        public ushort ID { get; set; }

        public Pixel(ushort id)
        {
            ID = id;
        }
    }
}
