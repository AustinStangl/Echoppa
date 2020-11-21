using Plugin.BLE.Abstractions.Contracts;
using System;
using System.Text;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using XamarinEssentials = Xamarin.Essentials;

namespace Echop.Client
{
    
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ThrottlePage : ContentPage
    {

        //private readonly IDevice _connectedDevice;
        public ThrottlePage(string throttleData)
        {
            InitializeComponent();
            
            Throttle.Text = throttleData;
            
            //_connectedDevice = connectedDevice;
            // InitButton.IsEnabled = !(ScanButton.IsEnabled = false);
        }
        private void ThrottleUpdate()
        {
           // Throttle.Text = throttleData;
            // Throttle.Text = ttd;
        }
    }
    }
