using Plugin.BLE.Abstractions.Contracts;
using System;
using System.Text;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using XamarinEssentials = Xamarin.Essentials;
using Microcharts;
using SkiaSharp;
using SkiaSharp.Views.Forms;

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

            //make dial ring
            DialRing.MoveTo(-60, 60);
            DialRing.LineTo(-70, 70);
            DialRing.ArcTo(new SKPoint(-70, 70), 0, SKPathArcSize.Large, SKPathDirection.Clockwise, new SKPoint(70, 70));
            DialRing.LineTo(60, 60);
            DialRing.ArcTo(new SKPoint(60, 60), 0, SKPathArcSize.Large, SKPathDirection.CounterClockwise, new SKPoint(-60, 60));
            DialRing.Close();


            //set a 60Hz refresh
            Device.StartTimer(TimeSpan.FromSeconds(1f / 60), () =>
                {
                    throttleCanvas.InvalidateSurface();
                    return true;
                });


            // InitButton.IsEnabled = !(ScanButton.IsEnabled = false);
        }

        private ICharacteristic sendCharacteristic;
        private ICharacteristic receiveCharacteristic;
        private string btdata, throttleData, busData, currentData, freqData, tempData, targetvData, switchFreqData, regSafetyInData, driverStateData, directionData, flagData, startCheck;
        private float busD, currentD, freqD, tempD, targetvD, switchFreqD, regSafetyInD, driverStateD, directionD, flagD, flag1D, flag2D, flag3D, flag4D;

        private float throttleD = 0, throttlePosition = 0;

        SKPaint tanFillPaint = new SKPaint
        {
            Style = SKPaintStyle.Fill,
            Color = SKColor.Parse("#f2efdb")
        };

        SKPaint orangeStrokePaint = new SKPaint
        {
            Style = SKPaintStyle.Fill,
            Color = SKColor.Parse("#fda47e"),
            StrokeWidth = 5,
            StrokeCap = SKStrokeCap.Round,
            IsAntialias = true
        };

        SKPaint grayFillPaint = new SKPaint
        {
            Style = SKPaintStyle.Fill,
            Color = SKColor.Parse("#8c8a8b"),
            IsAntialias = true
        };

        SKPath DialRing = new SKPath();


        private void throttleCanvas_PaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            //https://docs.microsoft.com/en-us/xamarin/xamarin-forms/user-interface/graphics/skiasharp/ was very helpful
            SKSurface surface = e.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear();

            //set transform
            int height = e.Info.Height;
            int width = e.Info.Width;

            canvas.Translate(width / 2, height / 2);
            canvas.Scale(width / 200);

            //indicator Background
            canvas.DrawCircle(0, 0, 100, tanFillPaint);

            //indicator Dial
            //canvas.DrawPath(DialRing, grayFillPaint);

            throttlePosition += (throttleD - throttlePosition) / (float)6.0;

            //needle
            canvas.Save();
            canvas.RotateDegrees(270f * throttlePosition/100f);
            canvas.DrawLine(0, 0, -70, 70, orangeStrokePaint);
            canvas.Restore();
        }


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
                                if(btdata.Substring(0, 3) != "EEE")
                                {
                                    btdata = null;
                                }

                                //----------Uncomment the lines below to display the Datas length-----------\\
                                //tring btData = receivedBytes.Length.ToString();
                                // RawData.Text = btdata;

                                //----------Uncomment the lines below to display the RawData----------\\
                                //RawData.Text = btdata;
                                var dataLength = btdata.Length;
                                if (dataLength >= 39 && (btdata.Substring(37, 2) == "GG"))     //Checks to make sure the recived data is the correct length
                                {
                                    dataParse();
                                    formatTheData();
                                    InitButton.BackgroundColor = Color.Transparent;
                                    btdata = null;          //Removes previous string after parsing data\
                                }
                                else if(dataLength >= 39)
                                {
                                    btdata = null;
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

        private void ThrottleButton_Clicked(object sender, EventArgs e)
        {
           //  Navigation.PushAsync(new ThrottlePage(Throttle.Text));
        }
        private void dataParse()
        {
            //Throttle.Text = btdata.Substring(0, 3);
            startCheck = btdata.Substring(0, 3);
            throttleData = btdata.Substring(6, 4);
            busData = btdata.Substring(10, 4);
            currentData = btdata.Substring(14, 4);
            switchFreqData = btdata.Substring(18, 4);
            tempData = btdata.Substring(22, 4);
            targetvData = btdata.Substring(26, 4);
            directionData = btdata.Substring(30, 1);
            regSafetyInData = btdata.Substring(31, 1);
            driverStateData = btdata.Substring(32, 1);
            flagData = btdata.Substring(33, 4);

        }

        private void formatTheData()
        {

            throttleD = (float.Parse(throttleData)) / 10;
            ThrottleBar.ProgressTo(throttleD/100, 100, Easing.Linear);
            Throttle.Text = "Throttle: " + (throttleD.ToString()) + "%";

            busD = (float.Parse(busData)) / 10;
            BusVoltage.Text = "Bus Voltage: " + (busD.ToString()) + "V";

            currentD = (float.Parse(currentData)) / 10;
            TotalCurrent.Text = "Total Current:" + (currentD.ToString()) + "A";

            switchFreqD = (float.Parse(switchFreqData)) / 10;
            Frequency.Text = "Freq: " + (switchFreqD.ToString()) + "Hz";

            tempD = (float.Parse(tempData)) / 10;
            Temp.Text = "Tempurature: " + (tempD.ToString()) + "C";
            if (tempD <= 79)
            { 
                Temp.BackgroundColor = Color.Green;
                Temp.TextColor = Color.White;
            }
            if (tempD >= 80 && tempD <= 99)
            {
                Temp.BackgroundColor = Color.Yellow;
                Temp.TextColor = Color.Black;
            }
            if (tempD >= 100)
            {
                Temp.BackgroundColor = Color.Red;
                Temp.TextColor = Color.White;
            }

            targetvD = (float.Parse(targetvData)) / 10;
            TargetVoltage.Text = "Target Voltage: " + (targetvD.ToString()) + "V";

            directionD = (float.Parse(directionData));
            if (regSafetyInD == 0)
            {
                Direction.Text = "Direction: Forward";
            }
            else
            {
                Direction.Text = "Direction: Reverse";
            }
            

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
            FlagsData();

        }
        
        private void FlagsData()
            {
            string flag1 = flagData.Substring(3, 1);
            string flag2 = flagData.Substring(2, 1);
            string flag3 = flagData.Substring(1, 1);
            string flag4 = flagData.Substring(0, 1);

            string flagD1, flagD2, flagD3, flagD4;

            if (flag1 == "1")
            {
                flagD1 = "Under Voltage " + Environment.NewLine;
            }

            else
            {
                flagD1 = "";
            }

            if (flag2 == "1")
            {
                // Flags.Text += "Yes%/n";
                flagD2 = "Over Voltage " + Environment.NewLine;
            }
            else
            {
                flagD2 = "";
            }

            if (flag3 == "1")
            {
                // Flags.Text += "Yes%/n";
                flagD3 = "High Current " + Environment.NewLine;
            }
            else
            {
                flagD3 = "";
            }

            if (flag4 == "1")
            {
                //Flags.Text += "Yes%/n";
                flagD4 = "Over Current " + Environment.NewLine;
            }
            else
            {
                flagD4 = "";
            }

            Flags.Text = flagD1 + flagD2 + flagD3 + flagD4;
        }

    }
}