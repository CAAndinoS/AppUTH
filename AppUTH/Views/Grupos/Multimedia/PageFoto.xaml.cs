using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace AppUTH.Views.Grupos.Multimedia
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class PageFoto : ContentPage
	{
        private string photoUrl;
        private string localImagePath;
        public PageFoto (string photoUrl)
		{
			InitializeComponent ();
            this.photoUrl = photoUrl;
            localImagePath = GetLocalImagePath();
            LoadImageFromLocalOrUrl();
        }
        private string GetLocalImagePath()
        {
            // Crear un nombre de archivo local basado en la URL
            // Podrías usar algún método de hashing para hacerlo más único si lo deseas
            var fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Path.GetFileName(photoUrl));
            return fileName;
        }

        private async void LoadImageFromLocalOrUrl()
        {
            try
            {
                // Verificar si la imagen ya está descargada localmente
                if (File.Exists(localImagePath))
                {
                    // Cargar la imagen desde el almacenamiento local
                    imagePreview.Source = ImageSource.FromFile(localImagePath);
                }
                else
                {
                    // Mostrar la barra de progreso
                    progressBar.IsVisible = true;
                    progressBar.IsEnabled = true;

                    // Descargar la imagen como una matriz de bytes (byte[])
                    using (var httpClient = new HttpClient())
                    {
                        var imageBytes = await httpClient.GetByteArrayAsync(photoUrl);
                        // Guardar la imagen en el almacenamiento local
                        File.WriteAllBytes(localImagePath, imageBytes);
                        // Crear un objeto Stream a partir de la matriz de bytes
                        var imageStream = new MemoryStream(imageBytes);
                        // Crear un objeto ImageSource desde el Stream
                        var imageSource = ImageSource.FromStream(() => imageStream);
                        // Asignar la imagen descargada al Image
                        imagePreview.Source = imageSource;
                    }

                    // Ocultar la barra de progreso
                    progressBar.IsVisible = false;
                    progressBar.IsEnabled = false;
                }
            }
            catch (Exception ex)
            {
                // Manejar cualquier error de descarga aquí
                await DisplayAlert("Error", $"No se pudo cargar la imagen: {ex.Message}", "OK");
                // Ocultar la barra de progreso en caso de error
                progressBar.IsVisible = false;
                progressBar.IsEnabled = false;
            }
        }
    }
}