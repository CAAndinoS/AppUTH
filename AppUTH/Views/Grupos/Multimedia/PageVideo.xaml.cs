using MediaManager;
using Octane.Xamarin.Forms.VideoPlayer;
using Plugin.Media.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace AppUTH.Views.Grupos.Multimedia
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class PageVideo : ContentPage
	{
        private string videoUrl;
        private string localVideoPath;
        public PageVideo (string videoUrl)
		{
			InitializeComponent ();

            this.videoUrl = videoUrl;
            localVideoPath = "video.mp4"; // Establece la ruta local del archivo de video si ya está descargado.

            if (File.Exists(localVideoPath))
            {
                // Si el archivo de video ya existe localmente, cargarlo directamente.
                videoPlayer.Source = localVideoPath;
            }
            else
            {
                // Si el archivo no existe localmente, cargarlo desde la URL proporcionada.
                videoPlayer.Source = new UriVideoSource
                {
                    Uri = new Uri(videoUrl)
                };
            }

        }
        protected override void OnAppearing()
        {
            base.OnAppearing();
            CrossMediaManager.Current.MediaItemFinished += OnMediaItemFinished;
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            CrossMediaManager.Current.MediaItemFinished -= OnMediaItemFinished;
            CrossMediaManager.Current.Pause();
        }
        
        private void OnMediaItemFinished(object sender, MediaManager.Media.MediaItemEventArgs e)
        {
            // Aquí puedes manejar cualquier lógica que desees cuando se complete la reproducción de un elemento multimedia.
            // Por ejemplo, puedes reproducir automáticamente el siguiente video en la lista o mostrar un mensaje al usuario, etc.
            // En este ejemplo, simplemente se muestra un mensaje de alerta cuando se completa la reproducción.
            Device.BeginInvokeOnMainThread(async () =>
            {
                await DisplayAlert("Reproducción completa", "El video ha terminado de reproducirse.", "OK");
            });
        }
    }
}