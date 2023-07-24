using AppUTH.Models;
using AppUTH.Singleton;
using AppUTH.Views.Grupos.Multimedia;
using Firebase.Database;
using Firebase.Database.Query;
using Firebase.Storage;
using Newtonsoft.Json;
using Plugin.AudioRecorder;
using Plugin.Media;
using Plugin.Media.Abstractions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    public partial class PageGrupo : TabbedPage, INotifyPropertyChanged
    {
        private FirebaseClient firebaseClient = new FirebaseClient("https://apputh-b2336-default-rtdb.firebaseio.com/");
        private Models.Group grupoSeleccionado;
        public  event PropertyChangedEventHandler PropertyChanged;     
        Plugin.Media.Abstractions.MediaFile photo = null;        
        String video = "";
        private AudioRecorderService audioRecorderService = new AudioRecorderService();
        private readonly AudioPlayer audioPlayer = new AudioPlayer();
        public string pathaudio, filename;
        private Command viewMultimediaCommand;
        public Command ViewMultimediaCommand => viewMultimediaCommand ?? (viewMultimediaCommand = new Command(OnViewMultimediaCommand));
        private Command deleteMultimediaCommand;
        public Command DeleteMultimediaCommand => deleteMultimediaCommand ?? (deleteMultimediaCommand = new Command(OnDeleteMultimediaCommand));

        public PageGrupo(Models.Group grupoSeleccionado)
        {
            InitializeComponent();
            this.grupoSeleccionado = grupoSeleccionado;
            LabelNombreGrupo.Text = "Bienvenidos al grupo de: " + grupoSeleccionado.Name;
            LabelCantidadPersonas.Text = "Cantidad de personas en el grupo: " + grupoSeleccionado.Participants.Count;
            // Asignar el origen de datos al ListView
            listViewMultimedia.ItemsSource = grupoSeleccionado.Multimedia;
            // Establecer el BindingContext para los comandos
            BindingContext = this;
        }

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

            string currentUser = GetCurrentUser();

            // Subir la foto al almacenamiento de Firebase
            string fotoUrl = await UploadMultimediaToStorage(photo.GetStream(), "photos");

            if (string.IsNullOrEmpty(fotoUrl))
            {
                await DisplayAlert("Error", "Error al subir la foto al almacenamiento.", "OK");
                return;
            }

            // Agregar la nueva foto y su URL a la lista de multimedia del grupo
            grupoSeleccionado.Multimedia.Add(new MultimediaEntry
            {
                UploadedBy = currentUser,
                FileUrl = fotoUrl,
                Type = "Foto"
            });

            // Crear el objeto del grupo con la lista actualizada de multimedia
            Models.Group updatedGroup = new Models.Group
            {
                IdGrupo = grupoSeleccionado.IdGrupo,
                Name = grupoSeleccionado.Name,
                Participants = grupoSeleccionado.Participants,
                Multimedia = grupoSeleccionado.Multimedia
            };

            // Guardar el grupo actualizado en Firebase
            await UpdateGroup(updatedGroup);
            OnPropertyChanged(nameof(grupoSeleccionado.Multimedia));
            listViewMultimedia.ItemsSource = null;
            listViewMultimedia.ItemsSource = grupoSeleccionado.Multimedia;
            await DisplayAlert("Éxito", "Foto subida correctamente.", "OK");
        }


        private  void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

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

            string currentUser = GetCurrentUser();

            // Subir el video al almacenamiento de Firebase
            string videoUrl = await UploadMultimediaToStorage(new MemoryStream(File.ReadAllBytes(video)), "videos");

            if (string.IsNullOrEmpty(videoUrl))
            {
                await DisplayAlert("Error", "Error al subir el video al almacenamiento.", "OK");
                return;
            }

            // Agregar el nuevo video y su URL a la lista de multimedia del grupo
            grupoSeleccionado.Multimedia.Add(new MultimediaEntry
            {
                UploadedBy = currentUser,
                FileUrl = videoUrl,
                Type = "Video"
            });

            // Crear el objeto del grupo con la lista actualizada de multimedia
            Models.Group updatedGroup = new Models.Group
            {
                IdGrupo = grupoSeleccionado.IdGrupo,
                Name = grupoSeleccionado.Name,
                Participants = grupoSeleccionado.Participants,
                Multimedia = grupoSeleccionado.Multimedia
            };

            // Guardar el grupo actualizado en Firebase
            await UpdateGroup(updatedGroup);
            OnPropertyChanged(nameof(grupoSeleccionado.Multimedia));
            listViewMultimedia.ItemsSource = null;
            listViewMultimedia.ItemsSource = grupoSeleccionado.Multimedia;
            await DisplayAlert("Éxito", "Video subido correctamente.", "OK");
        }

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
            if (await DisplayAlert("Reproducir Audio", "¿Desea reproducir el audio grabado?", "Sí", "No"))
            {
                if (File.Exists(audioRecorderService.GetAudioFilePath()))
                {
                    audioPlayer.Play(audioRecorderService.GetAudioFilePath());
                }
                else
                {
                    await DisplayAlert("Error", "El archivo de audio no existe.", "OK");
                }
            }
        }

        private async void OnUploadAudioButtonClicked(object sender, EventArgs e)
        {
            if (!File.Exists(audioRecorderService.GetAudioFilePath()))
            {
                await DisplayAlert("Error", "No se ha grabado ningún audio.", "OK");
                return;
            }

            string currentUser = GetCurrentUser();

            // Subir el audio al almacenamiento de Firebase
            string audioUrl = await UploadMultimediaToStorage(new MemoryStream(File.ReadAllBytes(audioRecorderService.GetAudioFilePath())), "audios");

            if (string.IsNullOrEmpty(audioUrl))
            {
                await DisplayAlert("Error", "Error al subir el audio al almacenamiento.", "OK");
                return;
            }

            // Agregar el nuevo audio y su URL a la lista de multimedia del grupo
            grupoSeleccionado.Multimedia.Add(new MultimediaEntry
            {
                UploadedBy = currentUser,
                FileUrl = audioUrl,
                Type = "Audio"
            });

            // Crear el objeto del grupo con la lista actualizada de multimedia
            Models.Group updatedGroup = new Models.Group
            {
                IdGrupo = grupoSeleccionado.IdGrupo,
                Name = grupoSeleccionado.Name,
                Participants = grupoSeleccionado.Participants,
                Multimedia = grupoSeleccionado.Multimedia
            };

            // Guardar el grupo actualizado en Firebase
            await UpdateGroup(updatedGroup);
            OnPropertyChanged(nameof(grupoSeleccionado.Multimedia));
            listViewMultimedia.ItemsSource = null;
            listViewMultimedia.ItemsSource = grupoSeleccionado.Multimedia;
            await DisplayAlert("Éxito", "Audio subido correctamente.", "OK");
        }

        private string GetCurrentUser()
        {
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
                // Si el nombre del usuario no se encuentra, se puede mostrar un mensaje de error o no subir el archivo.
                // En este caso, simplemente utilizamos una cadena vacía para el nombre del usuario.
            }

            return currentUserName;
        }
        private async Task UpdateGroup(Models.Group group)
        {
            await firebaseClient.Child("grupos").Child(group.IdGrupo).PutAsync(JsonConvert.SerializeObject(group));
        }
        private async Task<string> UploadMultimediaToStorage(Stream fileStream, string folderName)
        {
            try
            {
                var firebaseStorage = new FirebaseStorage("apputh-b2336.appspot.com");
                var fileName = $"{folderName}/file_{DateTime.Now.Ticks}.dat";
                var storageReference = firebaseStorage.Child(fileName);

                var task = await storageReference.PutAsync(fileStream);
                return await storageReference.GetDownloadUrlAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al subir el archivo al almacenamiento: {ex.Message}");
                return null;
            }
        }
        private async void OnViewMultimediaCommand(object multimedia)
        {
            var selectedMultimedia = multimedia as MultimediaEntry;

            // Verificar el tipo del archivo multimedia seleccionado y abrir la página correspondiente.
            switch (selectedMultimedia.Type)
            {
                case "Foto":
                    await Navigation.PushAsync(new PageFoto(selectedMultimedia.FileUrl));
                    break;
                case "Video":
                    await Navigation.PushAsync(new PageVideo(selectedMultimedia.FileUrl));
                    break;
                case "Audio":
                    await Navigation.PushAsync(new PageAudio(selectedMultimedia.FileUrl));
                    break;
                default:
                    await DisplayAlert("Error", "Tipo de archivo multimedia no válido.", "OK");
                    break;
            }
        }
       
        private async void OnDeleteMultimediaCommand(object multimedia)
        {
            var selectedMultimedia = multimedia as MultimediaEntry;

            bool result = await DisplayAlert("Confirmar Eliminación", "¿Estás seguro de que deseas eliminar este multimedia?", "Sí", "No");

            if (result)
            {
                grupoSeleccionado.Multimedia.Remove(selectedMultimedia);

                // Eliminar el elemento multimedia de Firebase Storage y la base de datos
                if (!string.IsNullOrEmpty(selectedMultimedia.FileUrl))
                {
                    if (selectedMultimedia.FileUrl.StartsWith("https://"))
                    {
                        await DeleteMultimediaFromStorage(selectedMultimedia.FileUrl);
                    }

                    // Crear el objeto del grupo con la lista actualizada de multimedia
                    Models.Group updatedGroup = new Models.Group
                    {
                        IdGrupo = grupoSeleccionado.IdGrupo,
                        Name = grupoSeleccionado.Name,
                        Participants = grupoSeleccionado.Participants,
                        Multimedia = grupoSeleccionado.Multimedia
                    };

                    // Guardar el grupo actualizado en Firebase
                    await UpdateGroup(updatedGroup);

                    // Eliminar la entrada de multimedia de la base de datos de Firebase Realtime Database
                    await firebaseClient
                        .Child("grupos")
                        .Child(grupoSeleccionado.IdGrupo)
                        .Child("Multimedia")
                        .Child(selectedMultimedia.FileUrl.GetHashCode().ToString()) // Utilizamos el hash de la URL como clave del nodo
                        .DeleteAsync();

                    // Notificar a la interfaz de usuario que la lista ha sido actualizada
                    OnPropertyChanged(nameof(grupoSeleccionado.Multimedia));
                    listViewMultimedia.ItemsSource = null;
                    listViewMultimedia.ItemsSource = grupoSeleccionado.Multimedia;
                }
            }
            else
            {
                // El usuario canceló la eliminación, no hacemos nada.
            }
        }

        private async Task DeleteMultimediaFromStorage(string fileUrl)
        {
            try
            {
                // Crear una instancia del cliente de Firebase Storage
                var firebaseStorage = new FirebaseStorage("apputh-b2336.appspot.com");

                // Obtener la referencia al archivo multimedia en Firebase Storage
                var storageReference = firebaseStorage.Child(fileUrl);

                // Eliminar el archivo multimedia de Firebase Storage
                await storageReference.DeleteAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al eliminar el archivo del almacenamiento: {ex.Message}");
            }
        }

    }
}