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
	public partial class PageRegistro : ContentPage
	{
        UsuarioRepositorio _usuarioRepositorio = new UsuarioRepositorio();
        public PageRegistro ()
		{
			InitializeComponent ();
		}

        private async void Buttonregistro_Clicked(object sender, EventArgs e)
        {
            try
            {

                string nombre = txtnombre.Text;
                string email = txtemail.Text.Trim();
                string clave = txtclave.Text.Trim();
                string confirclave = txtconfirmclave.Text.Trim();

                if (String.IsNullOrEmpty(nombre))
                {
                    await DisplayAlert("Aviso", "Escribe Un Nombre", "OK");
                    return;
                }
                if (String.IsNullOrEmpty(email))
                {
                    await DisplayAlert("Aviso", "Escribe Un Email", "OK");
                    return;
                }
                if (clave.Length < 6)
                {
                    await DisplayAlert("Aviso", "La contraseña tiene que contener al menos de 6 dígitos", "OK");
                    return;
                }
                if (String.IsNullOrEmpty(clave))
                {
                    await DisplayAlert("Aviso", "Escribe Una Clave", "OK");
                    return;
                }
                if (String.IsNullOrEmpty(confirclave))
                {
                    await DisplayAlert("Aviso", "Confirma Contraseña", "OK");
                    return;
                }
                if (clave != confirclave)
                {
                    await DisplayAlert("Aviso", "Contraseña No Coincide", "OK");
                    return;
                }

                bool IsSave = await _usuarioRepositorio.Register(email, nombre, clave);
                if (IsSave)
                {
                    Models.Usuario user = new Models.Usuario();
                    user.Name = nombre;
                    user.Correo = email;
                    user.Clave = clave;
                    user.Rol = "ALUMNO";
                    user.Estado = "ACTIVO";
                    var isSaves = await _usuarioRepositorio.Save(user);

                    if (isSaves)
                    {
                        await DisplayAlert("Aviso", "Registro Completo", "OK");
                        await Navigation.PopModalAsync();
                    }
                }
                else
                {
                    await DisplayAlert("Aviso", "Registro Fallo", "OK");
                }
            }
            catch (Exception exception)
            {
                if (exception.Message.Contains("EMAIL_EXISTS"))
                {
                    await DisplayAlert("Aviso", "Correo Ya Existe", "OK");
                }
                else
                {
                    await DisplayAlert("Aviso", exception.Message, "OK");
                }
            }
        }

        private async void Buttonlogin_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
        }
    }
}