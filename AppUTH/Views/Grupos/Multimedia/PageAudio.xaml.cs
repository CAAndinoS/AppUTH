using MediaManager;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration.TizenSpecific;
using Xamarin.Forms.Xaml;

namespace AppUTH.Views.Grupos.Multimedia
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class PageAudio : ContentPage
	{
        private string audioUrl;
        public PageAudio (string audioUrl)
		{
			InitializeComponent ();
            this.audioUrl = audioUrl;
        }
        private async void PlayAudio_Clicked(object sender, EventArgs e)
        {
            try
            {
                // Iniciar la reproducción de audio
                await CrossMediaManager.Current.Play(audioUrl);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"No se pudo reproducir el archivo de audio: {ex.Message}", "OK");
            }
        }

        private void StopAudio_Clicked(object sender, EventArgs e)
        {
            // Detener la reproducción de audio
            CrossMediaManager.Current.Stop();
        }

        private async void OnDownloadButtonClicked(object sender, EventArgs e)
        {
            try
            {
                // Verificar si el archivo ya ha sido descargado previamente
                var fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "audio.mp3");
                if (File.Exists(fileName))
                {
                    var answer = await DisplayAlert("Descarga previa", "Ya se ha descargado el archivo previamente. ¿Desea descargarlo nuevamente?", "Sí", "No");
                    if (!answer)
                        return;
                }

                // Crear una instancia de HttpClient para descargar el archivo
                using (var httpClient = new HttpClient())
                {
                    // Obtener la longitud del contenido para mostrar la barra de progreso
                    var contentLength = await GetContentLength(httpClient);

                    // Descargar el archivo como una matriz de bytes (byte[])
                    var audioBytes = await DownloadAudioFile(httpClient, contentLength);

                    // Guardar el archivo en el almacenamiento local
                    File.WriteAllBytes(fileName, audioBytes);

                    await DisplayAlert("Éxito", "El archivo de audio se ha descargado correctamente.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"No se pudo descargar el archivo de audio: {ex.Message}", "OK");
            }
        }

        private async Task<long> GetContentLength(HttpClient httpClient)
        {
            using (var response = await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, audioUrl)))
            {
                response.EnsureSuccessStatusCode();
                return response.Content.Headers.ContentLength ?? -1;
            }
        }

        private async Task<byte[]> DownloadAudioFile(HttpClient httpClient, long contentLength)
        {
            var audioBytes = new byte[contentLength];
            var totalRead = 0L;

            using (var stream = await httpClient.GetStreamAsync(audioUrl))
            {
                using (var outputStream = new MemoryStream(audioBytes))
                {
                    var buffer = new byte[4096];
                    var isMoreToRead = true;

                    do
                    {
                        var read = await stream.ReadAsync(buffer, 0, buffer.Length);
                        if (read == 0)
                        {
                            isMoreToRead = false;
                        }
                        else
                        {
                            await outputStream.WriteAsync(buffer, 0, read);

                            totalRead += read;

                            // Actualizar la barra de progreso
                            var progress = (double)totalRead / contentLength;
                            progressBar.Progress = progress;
                        }
                    } while (isMoreToRead);
                }
            }

            return audioBytes;
        }
    }
}
