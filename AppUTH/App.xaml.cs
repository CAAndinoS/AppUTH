using AppUTH.Singleton;
using AppUTH.Views;
using System;
using Xamarin.Essentials;
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
            base.OnStart();

            if (Preferences.ContainsKey("token"))
            {
                UserData.CurrentUserEmail = Preferences.Get("userEmail", string.Empty);
                UserData.FirebaseToken = Preferences.Get("token", string.Empty);
            }
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
