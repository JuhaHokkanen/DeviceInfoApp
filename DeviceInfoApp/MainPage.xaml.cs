using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Networking;
using Microsoft.Maui.Storage;
using Microcharts;
using SkiaSharp;

#if ANDROID
using Android.App;
using AndroidApp = Android.App.Application;
using Android.Content;
using Android.OS;
using Android.Net;                  // TrafficStats

#endif

namespace DeviceInfoApp
{
    public partial class MainPage : TabbedPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            InitializeBatteryMonitoring();
            LoadDeviceInfo();
            LoadStorageInfo();
            LoadMemoryInfo();
            LoadNetworkInfo();
            LoadDisplayInfo();
        }


#if ANDROID
        private async void TestNetworkButton_Clicked(object sender, EventArgs e)
        {
            int uid = Android.OS.Process.MyUid();
      
            long rxBefore = TrafficStats.GetUidRxBytes(uid);
            long txBefore = TrafficStats.GetUidTxBytes(uid);

            await Task.Delay(1000);
                      
            long rxAfter = TrafficStats.GetUidRxBytes(uid);
            long txAfter = TrafficStats.GetUidTxBytes(uid);

            // Laske nopeudet (bytes/s → KB/s)
            double rxRate = (rxAfter - rxBefore) / 1024.0;
            double txRate = (txAfter - txBefore) / 1024.0;

            // Päivitä UI

            RxLabel.Text = $"Received: {rxRate:F1} KB/s";
            TxLabel.Text = $"Sent:     {txRate:F1} KB/s";
        }
#else
    private void TestNetworkButton_Clicked(object sender, EventArgs e)
    {
        // ei-Android–alustoille
        
        RxLabel.Text   = "Received: –";
        TxLabel.Text   = "Sent:     –";
    }
#endif
        void InitializeBatteryMonitoring()
        {
            UpdateBatteryInfo();
            Device.StartTimer(TimeSpan.FromSeconds(5), () =>
            {
                UpdateBatteryInfo();
                return true;
            });
        }

        void LoadDeviceInfo()
        {
            ManufacturerLabel.Text = $"Valmistaja: {DeviceInfo.Manufacturer}";
            ModelLabel.Text = $"Malli:      {DeviceInfo.Model}";
            OSVersionLabel.Text = $"Versio:     {DeviceInfo.VersionString}";
            CpuModelLabel.Text = $"Malli: {DeviceInfo.Model}";
            CpuArchLabel.Text = $"Alusta: {DeviceInfo.Platform} ({DeviceInfo.Idiom})";
        }

        void LoadStorageInfo()
        {
#if ANDROID
            var path = Android.OS.Environment.DataDirectory.AbsolutePath;
            var stat = new StatFs(path);
            double totalGb = stat.BlockCountLong * stat.BlockSizeLong / 1_000_000_000.0;
            double freeGb = stat.AvailableBlocksLong * stat.BlockSizeLong / 1_000_000_000.0;
            double usedGb = totalGb - freeGb;

            StorageTotalLabel.Text = $"Koko:  {totalGb:F1} GB";
            StorageFreeLabel.Text = $"Vapaa: {freeGb:F1} GB";

            StorageChartView.Chart = new DonutChart
            {
                Entries = new[]
                {
                    new ChartEntry((float)usedGb)
                    {
                        Label      = "Käytetty",
                        ValueLabel = $"{usedGb:F1} GB",
                        Color      = SKColor.Parse("#FF5722"),
                    },
                    new ChartEntry((float)freeGb)
                    {
                        Label      = "Vapaa",
                        ValueLabel = $"{freeGb:F1} GB",
                        Color      = SKColor.Parse("#4CAF50"),
                    }
                },
                LabelTextSize = 32
            };
#else
            StorageTotalLabel.Text = "Saatavilla vain Androidilla";
            StorageFreeLabel.Text  = string.Empty;
            StorageChartView.Chart = null;
#endif
        }

        void LoadMemoryInfo()
        {
#if ANDROID
            var activityManager = (ActivityManager)AndroidApp.Context.GetSystemService(Context.ActivityService);
            var memInfo = new ActivityManager.MemoryInfo();
            activityManager.GetMemoryInfo(memInfo);

            double totalMemGb = memInfo.TotalMem / 1_000_000_000.0;
            double availMemGb = memInfo.AvailMem / 1_000_000_000.0;
            double usedMemGb = totalMemGb - availMemGb;

            MemoryTotalLabel.Text = $"Kokonaismuisti: {totalMemGb:F1} GB";
            MemoryFreeLabel.Text = $"Vapaa muisti:   {availMemGb:F1} GB";

            MemoryChartView.Chart = new DonutChart
            {
                Entries = new[]
                {
                    new ChartEntry((float)usedMemGb)
                    {
                        Label      = "Käytetty",
                        ValueLabel = $"{usedMemGb:F1} GB",
                        Color      = SKColor.Parse("#FF5722"),
                    },
                    new ChartEntry((float)availMemGb)
                    {
                        Label      = "Vapaa",
                        ValueLabel = $"{availMemGb:F1} GB",
                        Color      = SKColor.Parse("#4CAF50"),
                    }
                },
                LabelTextSize = 30
            };
#else
            MemoryTotalLabel.Text = "Saatavilla vain Androidilla";
            MemoryFreeLabel.Text  = string.Empty;
#endif
        }

        void LoadNetworkInfo()
        {
            var access = Connectivity.Current.NetworkAccess;
            var profiles = string.Join(", ", Connectivity.Current.ConnectionProfiles);
            NetworkAccessLabel.Text = $"Pääsy: {access}";
            ConnectionProfilesLabel.Text = $"Profiilit: {profiles}";
        }

        void LoadDisplayInfo()
        {
            var display = DeviceDisplay.Current.MainDisplayInfo;
            DisplayResolutionLabel.Text = $"Resoluutio: {display.Width} x {display.Height}";
            DisplayDensityLabel.Text = $"Tiheys:    {display.Density:F1}";
        }

        void UpdateBatteryInfo()
        {
#if ANDROID
            var state = Battery.Default.State;
            var level = Battery.Default.ChargeLevel;

            BatteryStateLabel.Text = state switch
            {
                BatteryState.Charging => "Laturi kytketty, lataus käynnissä",
                BatteryState.Discharging => "Laturi ei kytketty, purkuu",
                BatteryState.Full => "Akku täynnä",
                BatteryState.NotCharging => "Akku ei lataudu",
                BatteryState.NotPresent => "Akku ei saatavilla",
                _ => "Akun tila tuntematon"
            };
            BatteryLevelLabel.Text = $"Akun varaus: {level * 100:F0}%";

            var ctx = Android.App.Application.Context;
            using var ifilter = new IntentFilter(Intent.ActionBatteryChanged);
            var battery = ctx.RegisterReceiver(null, ifilter);

            int tempTenths = battery.GetIntExtra(BatteryManager.ExtraTemperature, 0);
            double tempC = tempTenths / 10.0;
            BatteryTempLabel.Text = $"Lämpötila: {tempC:F1} °C";

            int voltMv = battery.GetIntExtra(BatteryManager.ExtraVoltage, 0);
            BatteryVoltageLabel.Text = $"Jännite: {voltMv} mV";
#else
            BatteryStateLabel.Text    = "Saatavilla vain Androidilla";
            BatteryLevelLabel.Text    = string.Empty;
            BatteryTempLabel.Text     = string.Empty;
            BatteryVoltageLabel.Text  = string.Empty;
#endif
        }
    }
}
