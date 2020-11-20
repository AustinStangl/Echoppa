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
        public ThrottlePage()
        {
            InitializeComponent();
            //_connectedDevice = connectedDevice;
            // InitButton.IsEnabled = !(ScanButton.IsEnabled = false);
        }
    }
    }
