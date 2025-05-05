using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DeviceInfoApp.Services;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Controls;


namespace DeviceInfoApp.Services
{
    public class CpuInfo
    {
        public string Model { get; set; }
        public string Architecture { get; set; }
    }

    public class StorageInfo
    {
        public long TotalBytes { get; set; }
        public long FreeBytes { get; set; }
    }


    public interface IDeviceInfoService
    {
        Task<CpuInfo> GetCpuInfoAsync();
        Task<StorageInfo> GetStorageInfoAsync();

    }
}
