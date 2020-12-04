using Plugin.BLE;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Xamarin.Forms;
using XamarinEssentials = Xamarin.Essentials;

namespace Echop.Client
{
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {
        private readonly IAdapter _bluetoothAdapter;
        private List<IDevice> _gattDevices = new List<IDevice>();

        public MainPage()
        {
            InitializeComponent();

            _bluetoothAdapter = CrossBluetoothLE.Current.Adapter;
            _bluetoothAdapter.DeviceDiscovered += (sender, foundBleDevice) =>
            {

                if (foundBleDevice.Device != null && !string.IsNullOrEmpty(foundBleDevice.Device.Name))
                    //if (foundBleDevice.Device.Name == "BTboi")
                    _gattDevices.Add(foundBleDevice.Device);


            };

        }

        private async Task<bool> PermissionsGrantedAsync()
        {
            var locationPermissionStatus = await XamarinEssentials.Permissions.CheckStatusAsync<XamarinEssentials.Permissions.LocationAlways>();

            if (locationPermissionStatus != XamarinEssentials.PermissionStatus.Granted)
            {
                var status = await XamarinEssentials.Permissions.RequestAsync<XamarinEssentials.Permissions.LocationAlways>();
                return status == XamarinEssentials.PermissionStatus.Granted;
            }
            return true;
        }

        private async void ScanButton_Clicked(object sender, EventArgs e)
        {
            if (foundBleDevicesListView.BackgroundColor == Color.Black)
            { foundBleDevicesListView.BackgroundColor = Color.Transparent; }

            // IsBusyIndicator.IsVisible = IsBusyIndicator.IsRunning = !(ScanButton.IsEnabled = false);
            
            
            foundBleDevicesListView.ItemsSource = null;
            ScanButton.Text = "Searching For Device";
            ScanButton.IsEnabled = false;
            
            ConnectionAnimation();
            if (!await PermissionsGrantedAsync())
            {
                await DisplayAlert("Permission required", "Application needs location permission", "OK");
            //    IsBusyIndicator.IsVisible = IsBusyIndicator.IsRunning = !(ScanButton.IsEnabled = true);
                return;
            }

            _gattDevices.Clear();

            foreach (var device in _bluetoothAdapter.ConnectedDevices)
                _gattDevices.Add(device);

            await _bluetoothAdapter.StartScanningForDevicesAsync();

            foundBleDevicesListView.ItemsSource = _gattDevices.ToArray();
            ConnectImage.Source = null;
           // foundBleDevicesListView.BackgroundColor = Color.Black;
         //   IsBusyIndicator.IsVisible = IsBusyIndicator.IsRunning = !(ScanButton.IsEnabled = true);
            ScanButton.Text = "Rescan"; 
            ScanButton.IsEnabled = true;
            
            if (ConnectImage.IsAnimationPlaying == true)
            {
                ConnectImage.IsAnimationPlaying = false;
            }


        }
        private async void ConnectionAnimation()
        {
            ConnectImage.Source = "Phone.gif";
            ConnectImage.IsAnimationPlaying = true;
            
        }
        private async void FoundBluetoothDevicesListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            
          //  IsBusyIndicator.IsVisible = IsBusyIndicator.IsRunning = !(ScanButton.IsEnabled = false);
            IDevice selectedItem = e.Item as IDevice;
            //ScanButton.Text = "Rescan";
            
            
            if (selectedItem.State == DeviceState.Connected)
            {
                await Navigation.PushAsync(new BluetoothDataPage(selectedItem));
            }
            else
            {
                try
                {
                    var connectParameters = new ConnectParameters(false, true);
                    await _bluetoothAdapter.ConnectToDeviceAsync(selectedItem, connectParameters);
                    await Navigation.PushAsync(new BluetoothDataPage(selectedItem));
                }
                catch
                {
                    await DisplayAlert("Error connecting", $"Error connecting to BLE device: {selectedItem.Name ?? "N/A"}", "Retry");
                }
            }

         //   IsBusyIndicator.IsVisible = IsBusyIndicator.IsRunning = !(ScanButton.IsEnabled = true);
        }
    }
}
