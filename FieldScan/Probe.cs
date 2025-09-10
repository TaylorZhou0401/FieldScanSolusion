using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FieldScan
{
    public class Probe
    {
        public string Name { get; set; }
        public List<AntennaFactorPoint> AntennaFactors { get; set; }

        public Probe()
        {
            AntennaFactors = new List<AntennaFactorPoint>();
        }

        // 通过线性插值计算给定频率下的天线因子
        public double GetFactorAtFrequency(double frequencyHz)
        {
            if (AntennaFactors == null || AntennaFactors.Count == 0)
            {
                return 0; // 如果没有校准数据，则返回0dB增益
            }

            // 排序以确保插值正确
            var sortedFactors = AntennaFactors.OrderBy(p => p.FrequencyHz).ToList();

            // 寻找两个最近的点进行线性插值
            var pointA = sortedFactors.LastOrDefault(p => p.FrequencyHz <= frequencyHz);
            var pointB = sortedFactors.FirstOrDefault(p => p.FrequencyHz >= frequencyHz);

            if (pointA == null) return pointB?.FactorDb ?? 0;
            if (pointB == null) return pointA.FactorDb;
            if (pointA == pointB) return pointA.FactorDb;

            // 线性插值公式
            double factor = pointA.FactorDb + (pointB.FactorDb - pointA.FactorDb) *
                ((frequencyHz - pointA.FrequencyHz) / (pointB.FrequencyHz - pointA.FrequencyHz));

            return factor;
        }
    }
}
