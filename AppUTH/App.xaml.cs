using AppUTH.Views;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace AppUTH
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new NavigationPage(new PageLogin());

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
