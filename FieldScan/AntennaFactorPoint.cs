using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FieldScan
{
    public class AntennaFactorPoint
    {
        // 频率，单位 Hz
        public double FrequencyHz { get; set; }

        // 天线因子，单位 dB(1/m)
        public double FactorDb { get; set; }
    }
}
