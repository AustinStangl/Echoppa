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
        private string btdata, throttleData, busData, currentData, freqData, tempData, targetvData, switchFreqData, regSafetyInData, driverStateData;
        private float throttleD, busD, currentD, freqD, tempD, targetvD, switchFreqD, regSafetyInD, driverStateD;



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
                                btdata += Encoding.UTF8.GetString(receivedBytes, 0, receivedBytes.Length); // + Environment.NewLine;


                                //----------Uncomment the lines below to display the Datas length-----------\\
                                //tring btData = receivedBytes.Length.ToString();
                                RawData.Text = btdata;

                                //----------Uncomment the lines below to display the RawData----------\\
                                //RawData.Text = btdata;
                                if (btdata.Length >= 35)     //Checks to make sure the recived data is the correct length
                                {
                                    dataParse();
                                    btdata = null;          //Removes previous string after parsing data\
                                    formatTheData();
                                }


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

        private async void ThrottleButton_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ThrottlePage());
        }
        private void dataParse()
        {
            //Throttle.Text = btdata.Substring(0, 3);
            throttleData = btdata.Substring(6, 4);
            busData = btdata.Substring(10, 4);
            currentData = btdata.Substring(14, 4);
            freqData = btdata.Substring(18, 4);
            tempData = btdata.Substring(22, 4);
            targetvData = btdata.Substring(26, 4);
            switchFreqData = btdata.Substring(30, 3);
            regSafetyInData = btdata.Substring(33, 1);
            driverStateData = btdata.Substring(34, 1);

        }

        private void formatTheData()
        {

            throttleD = (float.Parse(throttleData)) / 10;
            Throttle.Text = "Throttle: " + (throttleD.ToString()) + "%";

            busD = (float.Parse(busData)) / 10;
            BusVoltage.Text = "Bus Voltage: " + (busD.ToString()) + "V";

            currentD = (float.Parse(currentData)) / 10;
            TotalCurrent.Text = "Total Current: " + (currentD.ToString()) + "A";

            freqD = (float.Parse(freqData)) / 10;
            Frequency.Text = "Frequency " + (freqD.ToString()) + "Hz";

            tempD = (float.Parse(tempData)) / 10;
            Temp.Text = "Tempurature: " + (tempD.ToString()) + "C";

            targetvD = (float.Parse(targetvData)) / 10;
            TargetVoltage.Text = "Target Voltage: " + (targetvD.ToString()) + "V";

            switchFreqD = (float.Parse(switchFreqData)) / 10;
            SwitchingFreq.Text = "Switching Frequency: " + (switchFreqD.ToString()) + "A";

            regSafetyInD = (float.Parse(regSafetyInData));
            if (regSafetyInD == 1)
            {
                RegenSafetyIn.Text = "Regen Safety In: ON";
                RegenSafetyIn.BackgroundColor = Color.Green;
            }
            else
            {
                RegenSafetyIn.Text = "Regen Safety In: OFF";
                RegenSafetyIn.BackgroundColor = Color.Red;
            }

            driverStateD = (float.Parse(driverStateData));
            if (driverStateD == 1)
            {
                DriverState.Text = "Driver State: ON";
                DriverState.BackgroundColor = Color.Green;
            }
            else
            {
                DriverState.Text = "Driver State: OFF";
                DriverState.BackgroundColor = Color.Red;


                // driverStateD = (int.Parse(driverStateData));
                // DriverState.Text = "Driver State: " + (driverStateD.ToString());

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
}