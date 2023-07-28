using AppUTH.Models;
using AppUTH.Service;
using AppUTH.Singleton;
using Firebase.Database;
using Firebase.Database.Query;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace AppUTH.Views.Amigos
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PageMenuAmigos : TabbedPage, INotifyPropertyChanged
    {
        private ObservableCollection<Models.Alumno> alumnos;
        private ObservableCollection<Models.Alumno> alumnosFiltrados;
        private string textoBusqueda;
        private bool isSearching;
        private PerfilRepositorio perfilRepositorio = new PerfilRepositorio();
        public PageMenuAmigos ()
        {
            InitializeComponent();
            alumnos = new ObservableCollection<Models.Alumno>();
            alumnosFiltrados = new ObservableCollection<Models.Alumno>();
            listViewNombres.ItemsSource = alumnosFiltrados;
            BindingContext = this;
            CargarListaAmigos();
        }
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await CargarAlumnos();
            CargarListaAmigos();
            RealizarBusqueda();
        }

        private async Task CargarAlumnos()
        {
            try
            {
                // Obtener la lista de alumnos desde Firebase directamente
                var firebaseperfil = new FirebaseClient("https://apputh-b2336-default-rtdb.firebaseio.com/");
                var alumnosSnapshot = await firebaseperfil.Child("alumnos").OnceAsync<Models.Alumno>();

                // Limpiar la lista actual y cargar los nuevos alumnos
                alumnos.Clear();
                foreach (var alumnoSnapshot in alumnosSnapshot)
                {
                    var alumno = alumnoSnapshot.Object;
                    alumnos.Add(alumno);
                }
            }
            catch (Exception ex)
            {
                // Mostrar mensaje de error
                await DisplayAlert("Error", "Ocurrió un error al cargar los alumnos. Por favor, inténtalo de nuevo más tarde.", "Aceptar");

            }
        }

        private async void EntryBusqueda_TextChanged(object sender, TextChangedEventArgs e)
        {
            textoBusqueda = e.NewTextValue.ToLower();
            await RealizarBusqueda();
        }

        private async Task RealizarBusqueda()
        {
            IsSearching = true; // Mostrar el ActivityIndicator mientras se realiza la búsqueda
            await Task.Delay(500); // Simular una demora de 500 ms (opcional)

            alumnosFiltrados.Clear();

            if (string.IsNullOrWhiteSpace(textoBusqueda))
            {
                // Si el campo de búsqueda está vacío, mostrar todos los alumnos en la lista completa
                foreach (var alumno in alumnos)
                {
                    alumnosFiltrados.Add(alumno);
                }
            }
            else
            {
                // Filtrar la lista completa de alumnos por el nombre parcial ingresado
                foreach (var alumno in alumnos)
                {
                    if (alumno.NombreAlumno.ToLower().Contains(textoBusqueda))
                    {
                        alumnosFiltrados.Add(alumno);
                    }
                }
            }

            IsSearching = false; // Ocultar el ActivityIndicator después de la búsqueda
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public bool IsSearching
        {
            get { return isSearching; }
            set
            {
                if (isSearching != value)
                {
                    isSearching = value;
                    OnPropertyChanged(nameof(IsSearching));
                }
            }
        }

        private async void ButtonAgregarAmigo_Clicked(object sender, EventArgs e)
        {
            // Get the selected student (sender) from the binding context
            if (sender is Button button && button.CommandParameter is Models.Alumno selectedAlumno)
            {
                // Show a confirmation message to the user
                var result = await DisplayAlert("Agregar Amigo", $"¿Deseas agregar a {selectedAlumno.NombreAlumno} como amigo?", "Sí", "No");

                if (result) // If the user confirms the friend addition
                {
                    // Obtener el correo electrónico del usuario actual desde la clase UserData
                    string currentUserEmail = UserData.CurrentUserEmail;
                    // Obtener el perfil del usuario actual (creador del grupo)
                    Models.Alumno currentUserProfile = await perfilRepositorio.ObtenerAlumno(currentUserEmail);

                    // Check if the selected student is not the current logged-in student
                    if (currentUserProfile.IdAlumno == selectedAlumno.IdAlumno)
                    {
                        await DisplayAlert("Error", "No puedes agregarte a ti mismo como amigo.", "Aceptar");
                        return;
                    }

                    // Check if the selected student is already a friend of the current logged-in student
                    var isAlreadyFriend = currentUserProfile.ListaAmigos.Any(a => a.IdAmigo == selectedAlumno.IdAlumno);
                    if (isAlreadyFriend)
                    {
                        // If the selected student is already a friend, show a message to the user
                        await DisplayAlert("Amigo Existente", $"{selectedAlumno.NombreAlumno} ya es tu amigo.", "Aceptar");
                    }
                    else
                    {
                        // If the selected student is not already a friend, proceed to add them
                        currentUserProfile.ListaAmigos.Add(new Amigo { IdAmigo = selectedAlumno.IdAlumno, NombreAmigo = selectedAlumno.NombreAlumno });

                        // Guardar el perfil actualizado del Alumno actual
                        await perfilRepositorio.UpdatePerfil(currentUserProfile);

                        // Obtener el perfil completo del Alumno seleccionado
                        Models.Alumno selectedUserProfile = await perfilRepositorio.ObtenerAlumno(selectedAlumno.Correo);
                        // Actualizar su lista de amigos
                        selectedUserProfile.ListaAmigos.Add(new Amigo { IdAmigo = currentUserProfile.IdAlumno, NombreAmigo = currentUserProfile.NombreAlumno });

                        // Guardar el perfil actualizado del Alumno seleccionado
                        await perfilRepositorio.UpdatePerfil(selectedUserProfile);

                        CargarListaAmigos();

                        // Show a success message to the user
                        await DisplayAlert("Amigo Agregado", $"{selectedAlumno.NombreAlumno} ha sido agregado como amigo.", "Aceptar");
                    }
                }
            }
        }



        private async Task<bool> IsFriend(Models.Alumno student, Models.Alumno potentialFriend)
        {
            try
            {
                // Get the student's existing friends list from Firebase or any other data source
                var firebasePerfil = new FirebaseClient("https://apputh-b2336-default-rtdb.firebaseio.com/");
                var amigosSnapshot = await firebasePerfil.Child("amigos").Child(student.IdAlumno.ToString()).OnceSingleAsync<GrupoAmigos>();

                // Get the current list of friends or create a new one if it doesn't exist
                var amigosList = amigosSnapshot?.ListaAmigos ?? new List<Amigo>();

                // Check if the potentialFriend is already in the list
                return amigosList.Any(a => a.IdAmigo == potentialFriend.IdAlumno);
            }
            catch (Exception ex)
            {
                // Handle the exception (e.g., show an error message)
                await DisplayAlert("Error", "Ocurrió un error al verificar si el estudiante es amigo. Por favor, inténtalo de nuevo más tarde.", "Aceptar");
                return false;
            }
        }

        private async Task AddFriend(Models.Alumno student, Models.Alumno friend)
        {
            try
            {
                // Get the student's existing friends list from Firebase or any other data source
                var firebasePerfil = new FirebaseClient("https://apputh-b2336-default-rtdb.firebaseio.com/");
                var amigosSnapshot = await firebasePerfil.Child("amigos").Child(student.IdAlumno.ToString()).OnceAsync<GrupoAmigos>();

                // Get the current list of friends or create a new one if it doesn't exist
                var amigosList = amigosSnapshot.FirstOrDefault()?.Object?.ListaAmigos ?? new List<Amigo>();

                // Check if the friend is already in the list (to avoid duplicates)
                if (amigosList.Any(a => a.NombreAmigo == friend.NombreAlumno))
                    return;

                // Add the friend to the list
                amigosList.Add(new Amigo { IdAmigo = friend.IdAlumno, NombreAmigo = friend.NombreAlumno });

                // Update the friends list in Firebase
                await firebasePerfil.Child("amigos").Child(student.IdAlumno.ToString()).PutAsync(new GrupoAmigos
                {
                    IdAlumno = student.IdAlumno,
                    ListaAmigos = amigosList
                });
            }
            catch (Exception ex)
            {
                // Handle the exception (e.g., show an error message)
                await DisplayAlert("Error", "Ocurrió un error al agregar el amigo. Por favor, inténtalo de nuevo más tarde.", "Aceptar");
            }
        }

        private List<Amigo> _listaAmigos;
        public List<Amigo> ListaAmigos
        {
            get => _listaAmigos;
            set
            {
                _listaAmigos = value;
                OnPropertyChanged(); // Implementa INotifyPropertyChanged para notificar a la vista sobre cambios en la propiedad
            }
        }
        // Dentro del constructor de la página o donde cargues los datos
        private async void CargarListaAmigos()
        {
            try
            {
                // Obtener el correo electrónico del usuario actual desde la clase UserData
                string currentUserEmail = UserData.CurrentUserEmail;
                // Obtener el perfil del usuario actual (creador del grupo)
                Models.Alumno currentUserProfile = await perfilRepositorio.ObtenerAlumno(currentUserEmail);

                // Obtiene la lista actual de amigos del Alumno actual
                ListaAmigos = currentUserProfile.ListaAmigos ?? new List<Amigo>();

                // Actualiza el ListView con la nueva lista de amigos
                ListViewAmigos.ItemsSource = ListaAmigos;
            }
            catch (Exception ex)
            {
                // Maneja la excepción (por ejemplo, muestra un mensaje de error)
                await DisplayAlert("Error", "Ocurrió un error al cargar la lista de amigos. Por favor, inténtalo de nuevo más tarde.", "Aceptar");
            }
        }




    }
}
