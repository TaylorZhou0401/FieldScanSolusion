using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FieldScan
{
    public class InstrumentSettings
    {
        public string IpAddress { get; set; } = "192.168.0.22";
        public int Port { get; set; } = 5025;
        public double CenterFrequencyHz { get; set; } = 1e9; // 1 GHz
        public double SpanHz { get; set; } = 100e6; // 100 MHz
        public int Points { get; set; } = 1001;

        // --- 新增下面两个属性来保存单位 ---
        public string CenterFrequencyUnit { get; set; } = "GHz";
        public string SpanUnit { get; set; } = "MHz";
    }
}
