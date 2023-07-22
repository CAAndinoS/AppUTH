using AppUTH.Models;
using AppUTH.Singleton;
using Firebase.Database;
using Firebase.Database.Query;
using Firebase.Storage;
using Newtonsoft.Json;
using Plugin.AudioRecorder;
using Plugin.Media;
using Plugin.Media.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace AppUTH.Views.Grupos
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PageGrupo : TabbedPage
    {
        private FirebaseClient firebaseClient = new FirebaseClient("https://apputh-b2336-default-rtdb.firebaseio.com/");
        private Models.Group grupoSeleccionado;
        //-----
        Plugin.Media.Abstractions.MediaFile photo = null;
        
        String video = "";

        private AudioRecorderService audioRecorderService = new AudioRecorderService();
        private readonly AudioPlayer audioPlayer = new AudioPlayer();
        public string pathaudio, filename;

        public PageGrupo(Models.Group grupoSeleccionado)
        {
            InitializeComponent();
            this.grupoSeleccionado = grupoSeleccionado;
            LabelNombreGrupo.Text = grupoSeleccionado.Name;
            LabelCantidadPersonas.Text = "Cantidad de personas en el grupo: " + grupoSeleccionado.Participants.Count;
        }

        private async void OnCloseButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
        //-----------
        
        private async void OnTakePhotoButtonClicked(object sender, EventArgs e)
        {
            photo = await CrossMedia.Current.TakePhotoAsync(new Plugin.Media.Abstractions.StoreCameraMediaOptions
            {
                Directory = "MYAPP",
                Name = "Foto.jpg",
                SaveToAlbum = true
            });

            if (photo != null)
            {
                imagePreview.Source = ImageSource.FromStream(() => { return photo.GetStream(); });

            }
        }

        private async void OnUploadPhotoButtonClicked(object sender, EventArgs e)
        {
            if (photo == null)
            {
                await DisplayAlert("Error", "No se ha tomado ninguna foto.", "OK");
                return;
            }

            // Obtener el nombre del usuario actual desde la clase UserData
            string currentUserEmail = UserData.CurrentUserEmail;
            string currentUserName = string.Empty;

            // Obtener el nombre del usuario actual del grupo (si está presente)
            var currentUser = grupoSeleccionado.Participants.FirstOrDefault(p => p.CorreoAlumno == currentUserEmail);
            if (currentUser != null)
            {
                currentUserName = currentUser.NombreAlumno;
            }
            else
            {
                // Si el usuario no es un participante registrado del grupo, intentamos obtener su nombre desde Firebase u otras fuentes.
                // Esto depende de cómo esté configurada la aplicación y si el nombre del usuario se almacena en algún lugar accesible.
                // En este ejemplo, asumiremos que el nombre del usuario se obtiene de alguna otra manera.
                // Si el nombre del usuario no se encuentra, se puede mostrar un mensaje de error o no subir la foto.
                // En este caso, simplemente utilizamos una cadena vacía para el nombre del usuario.
            }

            // Subir la foto al almacenamiento de Firebase
            string fotoUrl = await UploadPhotoToStorage(photo.GetStream());

            if (string.IsNullOrEmpty(fotoUrl))
            {
                await DisplayAlert("Error", "Error al subir la foto al almacenamiento.", "OK");
                return;
            }

            // Agregar la nueva foto y su URL a la lista de fotos del grupo
            grupoSeleccionado.Photos.Add(new PhotoEntry
            {
                UploadedBy = currentUserName,
                FotoUrl = fotoUrl,
                Foto = "Foto"
            });

            // Crear el objeto del grupo con la lista actualizada de fotos
            Models.Group updatedGroup = new Models.Group
            {
                IdGrupo = grupoSeleccionado.IdGrupo,
                Name = grupoSeleccionado.Name,
                Participants = grupoSeleccionado.Participants,
                Photos = grupoSeleccionado.Photos
            };

            // Guardar el grupo actualizado en Firebase
            await UpdateGroup(updatedGroup);

            await DisplayAlert("Éxito", "Foto subida correctamente.", "OK");
        }

        private async Task<string> UploadPhotoToStorage(Stream photoStream)
        {
            try
            {
                var firebaseStorage = new FirebaseStorage("apputh-b2336.appspot.com");
                var storageReference = firebaseStorage.Child("photos").Child($"photo_{DateTime.Now.Ticks}.jpg");

                var task = await storageReference.PutAsync(photoStream);
                return await storageReference.GetDownloadUrlAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al subir la foto al almacenamiento: " + ex.Message);
                return null;
            }
        }



        private async Task UpdateGroup(Models.Group group)
        {
            await firebaseClient.Child("grupos").Child(group.IdGrupo).PutAsync(JsonConvert.SerializeObject(group));
        }

        //-----------------------------------------
        
        private async void OnTakeVideoButtonClicked(object sender, EventArgs e)
        {
            await CrossMedia.Current.Initialize();

            if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakeVideoSupported)
            {
                await DisplayAlert("AVISO", "La cámara no está disponible.", "OK");
                return;
            }

            var file = await CrossMedia.Current.TakeVideoAsync(new Plugin.Media.Abstractions.StoreVideoOptions
            {
                Name = "Vid01.mp4",
                Directory = "MyVideos"
            });

            if (file == null)
                return;

            await DisplayAlert("AVISO", "El video se ha guardado en: " + file.Path, "OK");
            videoPreview.Source = file.Path;
            video = file.Path; // Almacenar la ruta del archivo de video en la variable "video"
        }

        private async void OnUploadVideoButtonClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(video))
            {
                await DisplayAlert("Error", "No se ha capturado ningún video.", "OK");
                return;
            }

            // Obtener el nombre del usuario actual desde la clase UserData
            string currentUserEmail = UserData.CurrentUserEmail;
            string currentUserName = string.Empty;

            // Obtener el nombre del usuario actual del grupo (si está presente)
            var currentUser = grupoSeleccionado.Participants.FirstOrDefault(p => p.CorreoAlumno == currentUserEmail);
            if (currentUser != null)
            {
                currentUserName = currentUser.NombreAlumno;
            }
            else
            {
                // Si el usuario no es un participante registrado del grupo, intentamos obtener su nombre desde Firebase u otras fuentes.
                // Esto depende de cómo esté configurada la aplicación y si el nombre del usuario se almacena en algún lugar accesible.
                // En este ejemplo, asumiremos que el nombre del usuario se obtiene de alguna otra manera.
                // Si el nombre del usuario no se encuentra, se puede mostrar un mensaje de error o no subir el video.
                // En este caso, simplemente utilizamos una cadena vacía para el nombre del usuario.
            }

            // Subir el video al almacenamiento de Firebase
            string videoUrl = await UploadVideoToStorage(video);

            if (string.IsNullOrEmpty(videoUrl))
            {
                await DisplayAlert("Error", "Error al subir el video al almacenamiento.", "OK");
                return;
            }

            // Agregar el nuevo video y su URL a la lista de videos del grupo
            grupoSeleccionado.Videos.Add(new VideoEntry
            {
                UploadedBy = currentUserName,
                VideoUrl = videoUrl,
                Video = "Video"
            });

            // Crear el objeto del grupo con la lista actualizada de videos
            Models.Group updatedGroup = new Models.Group
            {
                IdGrupo = grupoSeleccionado.IdGrupo,
                Name = grupoSeleccionado.Name,
                Participants = grupoSeleccionado.Participants,
                Videos = grupoSeleccionado.Videos
            };

            // Guardar el grupo actualizado en Firebase
            await UpdateGroup(updatedGroup);

            await DisplayAlert("Éxito", "Video subido correctamente.", "OK");
        }

        private async Task<string> UploadVideoToStorage(string videoPath)
        {
            try
            {
                var stream = new MemoryStream(File.ReadAllBytes(videoPath));
                var firebaseStorage = new FirebaseStorage("apputh-b2336.appspot.com");
                var storageReference = firebaseStorage.Child("videos").Child(Path.GetFileName(videoPath));

                var task = await storageReference.PutAsync(stream);
                return await storageReference.GetDownloadUrlAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al subir el video al almacenamiento: " + ex.Message);
                return null;
            }
        }


        //-----------------------------------------
        private async void OnStartRecordingAudioButtonClicked(object sender, EventArgs e)
        {
            var permiso = await Permissions.RequestAsync<Permissions.Microphone>();
            var permiso1 = await Permissions.RequestAsync<Permissions.StorageRead>();
            var permiso2 = await Permissions.RequestAsync<Permissions.StorageWrite>();
            
            if (permiso != PermissionStatus.Granted & permiso1 != PermissionStatus.Granted & permiso2 != PermissionStatus.Granted)
            {
                return;

            }

            if (audioRecorderService.IsRecording)
            {
                await audioRecorderService.StopRecording();
                audioPlayer.Play(audioRecorderService.GetAudioFilePath());
            }
            else
            {
                await audioRecorderService.StartRecording();
            }
        }

        private async void OnStopRecordingAudioButtonClicked(object sender, EventArgs e)
        {
            if (audioRecorderService.IsRecording)
            {
                await audioRecorderService.StopRecording();               
            }
            else
            {
                await audioRecorderService.StartRecording();
            }
        }

        private async void OnPlayAudioButtonClicked(object sender, EventArgs e)
        {
            if (await DisplayAlert("Lista Audio", "Desea Reproducir Audio", "Si", "No"))
            {
                audioPlayer.Play(audioRecorderService.GetAudioFilePath());
            }
        }

        private void OnUploadAudioButtonClicked(object sender, EventArgs e)
        {

        }
    }
}