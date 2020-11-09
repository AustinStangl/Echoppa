using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Echop.Client
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            var navigationPage = new NavigationPage(new MainPage());
            MainPage = navigationPage;
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
