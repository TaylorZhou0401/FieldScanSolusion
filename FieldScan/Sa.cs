using Ivi.Visa;
using System;
using System.Threading;

namespace FieldScan
{
    public class Sa
    {
        private IMessageBasedSession session;
        private IMessageBasedFormattedIO formattedIO;
        private bool isConnected = false;

        // 修改Connect方法，接收IP和端口作为参数
        public void Connect(string ipAddress, int port)
        {
            try
            {
                Disconnect(); // 先断开旧的连接
                string visaAddress = $"TCPIP::{ipAddress}::{port}::SOCKET";
                var visaSession = GlobalResourceManager.Open(visaAddress);
                session = visaSession as IMessageBasedSession;
                formattedIO = session.FormattedIO;
                session.TimeoutMilliseconds = 30000;
                session.TerminationCharacterEnabled = true;
                session.TerminationCharacter = (byte)'\n';
                session.SendEndEnabled = false;
                isConnected = true;
            }
            catch (Exception ex)
            {
                isConnected = false;
                throw new Exception("频谱仪连接失败: " + ex.Message);
            }
        }

        public void Disconnect()
        {
            if (session != null)
            {
                session.Clear();
                session.Dispose();
                session = null;
            }
            isConnected = false;
        }

        // 新增方法：发送SCPI指令
        private void WriteLine(string cmd)
        {
            if (!isConnected) throw new Exception("频谱仪未连接！");
            try
            {
                formattedIO.WriteLine(cmd);
            }
            catch (Exception ex)
            {
                throw new Exception($"频谱仪指令 '{cmd}' 发送失败: {ex.Message}");
            }
        }

        // 新增方法：设置中心频率
        public void SetCenterFrequency(double freqHz)
        {
            WriteLine($":FREQuency:CENTer {freqHz}");
        }

        // 新增方法：设置频域宽度
        public void SetSpan(double spanHz)
        {
            WriteLine($":FREQuency:SPAN {spanHz}");
        }

        // 新增方法：设置频率点数
        public void SetSweepPoints(int points)
        {
            WriteLine($":SWEep:POINts {points}");
        }

        // 其他方法保持不变...
        public double ReNewMaxHoldAndRead(int delay)
        {
            WriteLine(":TRAC:TYPE MAXH;");
            Thread.Sleep(delay);
            WriteLine(":CALC:MARK:MAX;"); // 确保找到最大值
            return QueryDouble(":CALC:MARK:Y?;");
        }

        public string ReadTrace()
        {
            return QueryString("TRAC? TRACE1;");
        }

        private string QueryString(string cmd)
        {
            if (!isConnected) throw new Exception("频谱仪未连接！");
            try
            {
                formattedIO.WriteLine(cmd);
                return formattedIO.ReadString();
            }
            catch (Exception ex)
            {
                throw new Exception($"频谱仪查询 '{cmd}' 失败: {ex.Message}");
            }
        }

        private double QueryDouble(string cmd)
        {
            if (!isConnected) throw new Exception("频谱仪未连接！");
            try
            {
                formattedIO.WriteLine(cmd);
                return formattedIO.ReadLineDouble();
            }
            catch (Exception ex)
            {
                throw new Exception($"频谱仪查询 '{cmd}' 失败: {ex.Message}");
            }
        }

        // 新增方法：设置参考电平
        public void SetReferenceLevel(double levelDb)
        {
            // SCPI指令: DISPlay:WINDow:TRACe:Y:RLEVel <level>
            WriteLine($":DISP:WIND:TRAC:Y:RLEV {levelDb}");
        }

        // 新增方法：查询扫描时间
        public double GetSweepTimeMillis()
        {
            // SCPI指令: :SWEep:TIME?
            // 这个指令通常返回单位为秒(s)的扫描时间
            // 注意：QueryString返回的字符串末尾可能有换行符，需要Trim处理
            string sweepTimeString = QueryString(":SWE:TIME?").Trim();
            double sweepTimeSeconds = double.Parse(sweepTimeString);

            // 将秒转换为毫秒并返回
            return sweepTimeSeconds * 1000;
        }
    }
}