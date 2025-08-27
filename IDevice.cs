//这个接口定义了所有硬件设备都必须具备的通用属性和方法，比如名称、状态和连接/断开功能。
using System.ComponentModel;
using System.Threading.Tasks;

namespace FieldScan
{
    public enum DeviceStatus
    {
        Disconnected,
        Connected,
        Connecting,
        Error
    }

    public interface IDevice : INotifyPropertyChanged
    {
        string Name { get; }
        string Details { get; }
        DeviceStatus Status { get; }
        Task CheckStatusAsync(); // 异步检查状态的方法
    }
}