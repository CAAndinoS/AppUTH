using AppUTH.Service;
using Plugin.Media;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;


namespace AppUTH.Views.Alumno
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PagePerfilAlumno : ContentPage
    {
        public Models.Alumno perfil = new Models.Alumno();
        PerfilRepositorio _perfilRepositorio = new PerfilRepositorio();
        Plugin.Media.Abstractions.MediaFile photo = null;
        public PagePerfilAlumno()
        {
            InitializeComponent();
            txtemail.Text = Preferences.Get("userEmail", "default");
            setData();
        }

        public async void setData()
        {
            perfil = await _perfilRepositorio.ObtenerAlumno(txtemail.Text);
            if (perfil != null)
            {
                Foto.Source = ImageSource.FromStream(() => new MemoryStream(Convert.FromBase64String(perfil.Foto)));
                txtnombre.Text = perfil.NombreAlumno;
                txtdireccion.Text = perfil.Direccion;
                txttelefono.Text = perfil.Telefono;
            }
        }

        private string traeImagenToBase64()
        {
            if (photo != null)
            {
                using (MemoryStream memory = new MemoryStream())
                {
                    Stream stream = photo.GetStream();
                    stream.CopyTo(memory);
                    byte[] fotobyte = memory.ToArray();

                    byte[] imagenescalada = obtener_imagen_escalada(fotobyte, 50, 500, 500); // Ajusta los valores de ancho y alto según tus necesidades

                    string base64String = Convert.ToBase64String(imagenescalada);
                    return base64String;
                }
            }
            return null;
        }

        private byte[] obtener_imagen_escalada(byte[] imagen, int compresion, int nuevoAncho, int nuevoAlto)
        {
            using (SKBitmap originalBitmap = SKBitmap.Decode(imagen))
            {
                SKImageInfo info = new SKImageInfo(nuevoAncho, nuevoAlto);
                using (SKBitmap scaledBitmap = originalBitmap.Resize(info, SKFilterQuality.High))
                {
                    using (SKData compressedData = scaledBitmap.Encode(SKEncodedImageFormat.Jpeg, compresion))
                    {
                        return compressedData.ToArray();
                    }
                }
            }
        }


        private async void btnFoto_Clicked(object sender, EventArgs e)
        {
            await CrossMedia.Current.Initialize();

            var source = await Application.Current.MainPage.DisplayActionSheet(
                "Elige Una Opcion",
                "Cancelar",
                null,
                "Galeria",
                "Camara");

            if (source == "Cancelar")
            {
                photo = null;
                return;
            }
            if (source == "Camara")
            {
                photo = await CrossMedia.Current.TakePhotoAsync(new Plugin.Media.Abstractions.StoreCameraMediaOptions
                {
                    Directory = "FotosAplicacion",
                    Name = "PhotoAlbum.jpg",
                    SaveToAlbum = true
                });
            }
            else
            {
                photo = await CrossMedia.Current.PickPhotoAsync();
            }

            if (photo != null)
            {
                Foto.Source = ImageSource.FromStream(() =>
                {
                    var stream = photo.GetStream();
                    return stream;
                });
            }
        }

        public async void UpdateMethod()
        {
            Models.Alumno p;
            if (perfil != null)
            {
                p = new Models.Alumno
                {
                    NombreAlumno = txtnombre.Text,
                    Telefono = txttelefono.Text,
                    Direccion = txtdireccion.Text,
                    Correo = perfil.Correo,
                    IdAlumno = perfil.IdAlumno,
                    Foto = traeImagenToBase64()
                };

                await _perfilRepositorio.UpdatePerfil(p);
                await DisplayAlert("Info", "Datos actualizados con exito", "Ok");
                return;
            }
            p = new Models.Alumno
            {
                NombreAlumno = txtnombre.Text,
                Telefono = txttelefono.Text,
                Direccion = txtdireccion.Text,
                Correo = txtemail.Text,
                Foto = traeImagenToBase64(),
                IdAlumno = await _perfilRepositorio.ObtenerID_Alumno()
            };

            await _perfilRepositorio.Guardar_Alumno(p);
            await DisplayAlert("Info", "Datos guardados con exito", "Ok");

        }
        private async void Buttoactualizar_Clicked(object sender, EventArgs e)
        {
            if (txtdireccion.Text.Equals(""))
            {
                await DisplayAlert("Info", "Porfavor llenar el campo direccion", "Ok");
            }
            else if (txtemail.Text.Equals(""))
            {
                await DisplayAlert("Info", "Porfavor llenar el campo email", "Ok");
            }
            else if (txtnombre.Text.Equals(""))
            {
                await DisplayAlert("Info", "Porfavor llenar el campo nombre", "Ok");
            }
            else if (txttelefono.Text.Equals(""))
            {
                await DisplayAlert("Info", "Porfavor llenar el campo telefono", "Ok");
            }
            else if (traeImagenToBase64() == null)
            {
                await DisplayAlert("Info", "Porfavor tomar la fotografia", "Ok");
            }
            else
            {
                UpdateMethod();
            }
        }
    }
}