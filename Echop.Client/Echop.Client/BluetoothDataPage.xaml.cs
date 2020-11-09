using Plugin.BLE.Abstractions.Contracts;
using System;
using System.Text;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using XamarinEssentials = Xamarin.Essentials;

namespace Echop.Client
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class BluetoothDataPage : ContentPage
    {
        private readonly IDevice _connectedDevice;

        public BluetoothDataPage(IDevice connectedDevice)
        {
            InitializeComponent();
            _connectedDevice = connectedDevice;
            // InitButton.IsEnabled = !(ScanButton.IsEnabled = false);
        }

        private ICharacteristic sendCharacteristic;
        private ICharacteristic receiveCharacteristic;
        private string btdata;
        //Th, Bv, Ac, Fr, tc, Tv, Fs, Rs, Ds; //Throttle, BusVoltage, Total Current, Frequency, Regen/Safety In, Driver State

        //private double ThrottleData;

        private async void InitalizeCommandButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                var service = await _connectedDevice.GetServiceAsync(GattIdentifiers.UartGattServiceId);

                if (service != null)
                {
                    sendCharacteristic = await service.GetCharacteristicAsync(GattIdentifiers.UartGattCharacteristicSendId);

                    receiveCharacteristic = await service.GetCharacteristicAsync(GattIdentifiers.UartGattCharacteristicReceiveId);
                    if (receiveCharacteristic != null)
                    {
                        var descriptors = await receiveCharacteristic.GetDescriptorsAsync();

                        receiveCharacteristic.ValueUpdated += (o, args) =>
                        {
                            var receivedBytes = args.Characteristic.Value;
                            XamarinEssentials.MainThread.BeginInvokeOnMainThread(() =>
                            {
                                btdata += Encoding.UTF8.GetString(receivedBytes, 0, receivedBytes.Length) + Environment.NewLine;
                                // Output.Text = btdata;
                                dataParse();
                                // ThrottleData = Convert.ToDouble(ThrottleString);

                                // Output1.Text = "Throttle:"ThrottleData;
                                // Throttle.Text = btdata.Substring(5, 4);
                                // BusVoltage.Text = btdata.Substring(15, 4);
                                //TotalCurrent.Text = btdata.Substring(19, 4);
                                //Frequency.Text = btdata.Substring(5, 4);
                                //Temp.Text = btdata.Substring(5, 4);
                                //TargetVoltage.Text = btdata.Substring(5, 4);
                                //SwitchingFreq.Text = btdata.Substring(5, 4);
                                //RegenSafetyIn.Text = btdata.Substring(5, 4);
                                //DriverState.Text = btdata.Substring(5, 4);
                            });
                        };

                        await receiveCharacteristic.StartUpdatesAsync();
                        //  InitButton.IsEnabled = !(ScanButton.IsEnabled = true);
                    }
                }
                else
                {
                    // Output1.Text += "UART GATT service not found." + Environment.NewLine;
                }
            }
            catch
            {
                // Output1.Text += "Error initializing UART GATT service." + Environment.NewLine;
            }
        }
        private void dataParse()
        {
            Throttle.Text = btdata.Substring(5, 4);
            BusVoltage.Text = btdata.Substring(15, 4);
        }
        /*   private async void SendCommandButton_Clicked(object sender, EventArgs e)
               {
                   try
                   {
                       if (sendCharacteristic != null)
                       {
                         //  var bytes = await sendCharacteristic.WriteAsync(Encoding.ASCII.GetBytes($"{CommandTxt.Text}\r\n"));
                       }
                   }
                   catch
                   {
                       //Output1.Text += "Error sending comand to UART." + Environment.NewLine;
                   }
               }*/
    }
}
