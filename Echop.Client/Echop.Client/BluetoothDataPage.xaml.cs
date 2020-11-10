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


                                //----------Uncomment the lines below to display the Datas length-----------\\
                                //tring btData = receivedBytes.Length.ToString();
                                //RawData.Text = btData;

                                //----------Uncomment the lines below to display the RawData----------\\
                                //RawData.Text = btdata;

                                dataParse();
                                
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
           
            Throttle.Text = btdata.Substring(4, 4);
            BusVoltage.Text = btdata.Substring(10, 4);
            TotalCurrent.Text = btdata.Substring(14, 4);
            Frequency.Text = btdata.Substring(18, 3);
            
            //Temp.Text = btdata.Substring(21, 3);  //The Code doesnt seem to like any values above 20
            //TargetVoltage.Text = btdata.Substring(25, 5);
            //SwitchingFreq.Text = btdata.Substring(30, 4);
            //RegenSafetyIn.Text = btdata.Substring(34, 1);
            //DriverState.Text = btdata.Substring(35, 1); 
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
