﻿using Plugin.BLE.Abstractions.Contracts;
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
        private float busD, currentD, freqD, tempD, targetvD, switchFreqD, regSafetyInD, driverStateD, directionD, flagD;

        private float throttleD = 0;

        SKPaint tanFillPaint = new SKPaint
        {
            Style = SKPaintStyle.Fill,
            Color = SKColor.Parse("#f2efdbff")
        };

        SKPaint orangeFillPaint = new SKPaint
        {
            Style = SKPaintStyle.Fill,
            Color = SKColor.Parse("#fda47eff"),
            StrokeCap = SKStrokeCap.Round,
            StrokeWidth = 5,
            IsAntialias = true
        };


        private void throttleCanvas_PaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            //https://docs.microsoft.com/en-us/xamarin/xamarin-forms/user-interface/graphics/skiasharp/ was very helpful
            SKSurface surface = e.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear(SKColors.CornflowerBlue);

            //set transform
            int height = e.Info.Height;
            int width = e.Info.Width;

            canvas.Translate(width / 2, height / 2);
            canvas.Scale(width / 200);

            //indicator Background
            canvas.DrawCircle(0, 0, 100, tanFillPaint);

            //needle
            canvas.Save();
            canvas.RotateDegrees(270 * throttleD/100);
            orangeFillPaint.StrokeWidth = 15;
            canvas.DrawLine(0, 0, -50, 50, orangeFillPaint);
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
                                //RawData.Text = btdata;

                                //----------Uncomment the lines below to display the RawData----------\\
                                //RawData.Text = btdata;
                                if (btdata.Length >= 38)     //Checks to make sure the recived data is the correct length
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

        private void ThrottleButton_Clicked(object sender, EventArgs e)
        {
             Navigation.PushAsync(new ThrottlePage(Throttle.Text));
        }
        private void dataParse()
        {
            //Throttle.Text = btdata.Substring(0, 3);
            startCheck = btdata.Substring(0, 3);
            throttleData = btdata.Substring(6, 4);
            busData = btdata.Substring(10, 4);
            currentData = btdata.Substring(14, 4);
            freqData = btdata.Substring(18, 4);
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
            TotalCurrent.Text = "Total Current: " + (currentD.ToString()) + "A";


            tempD = (float.Parse(tempData)) / 10;
            Temp.Text = "Tempurature: " + (tempD.ToString()) + "C";

            targetvD = (float.Parse(targetvData)) / 10;
            TargetVoltage.Text = "Target Voltage: " + (targetvD.ToString()) + "V";

            directionD = (float.Parse(directionData));
            Direction.Text = "Direction: " + (directionD.ToString());

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

            flagD = (float.Parse(flagData));
            Flags.Text = "Flags: " + (flagD.ToString());
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