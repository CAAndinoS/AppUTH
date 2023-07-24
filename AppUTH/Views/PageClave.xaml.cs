using AppUTH.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace AppUTH.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PageClave : ContentPage
    {
        UsuarioRepositorio _usuarioRepositorio = new UsuarioRepositorio();
        public PageClave()
        {
            InitializeComponent();
        }
        private async void Buttonlink_Clicked(object sender, EventArgs e)
        {
            string email = txtemail.Text;
            if (string.IsNullOrEmpty(email))
            {
                await DisplayAlert("AVISO", "Por Favor Ingresa Tu Correo", "OK");
                return;
            }
            bool isSend = await _usuarioRepositorio.Resetpassword(email);
            if (isSend)
            {
                await DisplayAlert("AVISO RECUPERACION DE CLAVE", "Por Favor Revisa Tu Correo Para Restablecer Tu Contraseña", "OK");
                await Navigation.PopModalAsync();
            }
            if (isSend)
            {
                await DisplayAlert("AVISO RECUPERACION DE CLAVE", "Error Al Recuperar Contraseña", "OK");
            }
        }

        private async void Buttonlink_1_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
        }
    }
}
