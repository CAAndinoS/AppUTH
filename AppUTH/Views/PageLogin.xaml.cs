using AppUTH.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Firebase.Database;
using Xamarin.Essentials;

namespace AppUTH.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PageLogin : ContentPage
    {
        UsuarioRepositorio _usuarioRepositorio = new UsuarioRepositorio();
        public PageLogin()
        {
            InitializeComponent();
            bool hasKey = Preferences.ContainsKey("token");
            if (hasKey)
            {
                string token = Preferences.Get("token", "");

                if (!string.IsNullOrEmpty(token))
                {
                    Palyer(Preferences.Get("userEmail", ""));
                }
            }
        }
        public async void Palyer(string email)
        {
            string rol = await _usuarioRepositorio.Getrole(email);


            switch (rol)
            {
                case "ALUMNO":
                    await Navigation.PushAsync(new Views.Menu.PageMenu());
                    break;

                case "PROFESOR":
                    
                    break;

                case "ADMIN":
                    
                    break;
            }
        }

        private async void BtnSigIn_Clicked(object sender, EventArgs e)
        {

            try
            {
                string email = txtemail.Text;
                string clave = txtclave.Text;

                if (String.IsNullOrEmpty(email))
                {
                    await DisplayAlert("Aviso", "Escribe Tu Email", "OK");
                    return;
                }
                if (String.IsNullOrEmpty(clave))
                {
                    await DisplayAlert("Aviso", "Escribe Tu Clave", "OK");
                    return;
                }

                string token = await _usuarioRepositorio.SignIn(email, clave);

                if (!string.IsNullOrEmpty(token))
                {
                    Preferences.Set("token", token);
                    Preferences.Set("userEmail", email);
                    Palyer(email);
                }
                else
                {
                    await DisplayAlert("AVISO", "Inicio De Sesión Fallo", "OK");
                }
            }
            catch (Exception exception)
            {
                if (exception.Message.Contains("EMAIL_NOT_FOUND"))
                {
                    await DisplayAlert("AVISO", "Email No Funciona", "OK");
                }
                else if (exception.Message.Contains("INVALID_PASSWORD"))
                {
                    await DisplayAlert("AVISO", "Contraseña Incorrecta", "OK");
                }
                else
                {
                    await DisplayAlert("Error", exception.Message, "OK");
                }
            }
        }

        private async void registertap_Tapped(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new PageRegistro());

        }

        private async void recuperarclavetap_Tapped(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new PageClave());

        }
    }
}