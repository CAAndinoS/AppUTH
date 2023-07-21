﻿using AppUTH.Service;
using AppUTH.Singleton;
using Firebase.Database;
using Firebase.Database.Query;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace AppUTH.Views.Menu_Grupos
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PageMenuGrupos : TabbedPage
    {
        private FirebaseClient firebaseClient = new FirebaseClient("https://apputh-b2336-default-rtdb.firebaseio.com/");
        private UsuarioRepositorio usuarioRepositorio = new UsuarioRepositorio();
        private PerfilRepositorio perfilRepositorio = new PerfilRepositorio();

        public PageMenuGrupos()
        {
            InitializeComponent();
            LoadGroups();
            LoadGruposInscritos();
        }

        private async void OnCreateGroupClicked(object sender, EventArgs e)
        {
            string groupName = GroupNameEntry.Text;
            if (string.IsNullOrEmpty(groupName))
            {
                await DisplayAlert("Error", "Por favor, ingresa un nombre para el grupo.", "OK");
                return;
            }

            // Crear el grupo y guardar en Firebase
            Group group = new Group { Name = groupName };
            await SaveGroup(group);
            LoadGroups();
            await DisplayAlert("Éxito", "Grupo creado", "OK");

            /*            
            // Obtener el perfil del usuario actual (creador del grupo)
            string currentUserEmail = UserData.CurrentUserEmail;
            Models.Alumno currentUserProfile = await perfilRepositorio.ObtenerAlumno(currentUserEmail);

            // Agregar al creador del grupo a la lista de participantes
            if (currentUserProfile != null)
            {
                group.Participants.Add(new Participant { IdAlumno = currentUserProfile.IdAlumno, NombreAlumno = currentUserProfile.NombreAlumno });
                await UpdateGroup(group); // Guardar el grupo actualizado en Firebase
                await DisplayAlert("Éxito", "Grupo creado y te has unido como participante.", "OK");
                LoadGroups(); // Actualizar la lista de grupos después de crear uno nuevo
            }
            else
            {
                await DisplayAlert("Error", "No se pudo encontrar el perfil del usuario.", "OK");
            }
            */
        }

        private async Task SaveGroup(Group group)
        {
            var data = await firebaseClient.Child("grupos").PostAsync(JsonConvert.SerializeObject(group));
            if (!string.IsNullOrEmpty(data.Key))
            {
                group.IdGrupo = data.Key;
            }
        }

        //-----------------------------------------------------
        private async void OnUnirseInvoked(object sender, EventArgs e)
        {
            var swipeItem = (SwipeItem)sender;
            var grupoSeleccionado = (Group)swipeItem.CommandParameter;

            // Obtener el correo electrónico del usuario actual desde la clase UserData
            string currentUserEmail = UserData.CurrentUserEmail;

            // Verificar si el usuario ya es miembro del grupo
            bool esMiembro = grupoSeleccionado.Participants.Any(p => p.CorreoAlumno == currentUserEmail);


            if (esMiembro)
            {
                await DisplayAlert("Información", "Ya eres miembro del grupo.", "OK");
            }
            else
            {
                // Obtener el perfil del usuario actual (creador del grupo)
                Models.Alumno currentUserProfile = await perfilRepositorio.ObtenerAlumno(currentUserEmail);

                if (currentUserProfile != null)
                {
                    // Agregar al usuario actual como participante del grupo
                    grupoSeleccionado.Participants.Add(new Participant
                    {
                        IdAlumno = currentUserProfile.IdAlumno,
                        NombreAlumno = currentUserProfile.NombreAlumno,
                        CorreoAlumno = currentUserEmail
                    });

                    // Guardar el grupo actualizado en Firebase
                    await UpdateGroup(grupoSeleccionado);

                    await DisplayAlert("Éxito", "Te has unido al grupo.", "OK");
                    LoadGroups(); // Actualizar la lista de grupos después de unirse a uno nuevo
                    LoadGruposInscritos(); // Actualizar la lista de grupos en los que el usuario está inscrito
                }
                else
                {
                    await DisplayAlert("Error", "No se pudo encontrar el perfil del usuario.", "OK");
                }
            }
        }

        private async Task UpdateGroup(Group group)
        {
            await firebaseClient.Child("grupos").Child(group.IdGrupo).PutAsync(JsonConvert.SerializeObject(group));
        }


        private async void LoadGroups()
        {
            var gruposData = await firebaseClient.Child("grupos").OnceAsync<Group>();
            var grupos = gruposData.Select(grupoData =>
            {
                var grupo = grupoData.Object;
                grupo.IdGrupo = grupoData.Key;
                return grupo;
            }).ToList();

            // Actualizar la lista de grupos en la ListView
            ListaGrupos.ItemsSource = grupos;
        }
        //-----------------------------------------------------
        private async void LoadGruposInscritos()
        {
            // Obtener el correo electrónico del usuario actual desde la clase UserData
            string currentUserEmail = UserData.CurrentUserEmail;

            // Obtener los grupos en los que el usuario está inscrito
            var gruposData = await firebaseClient.Child("grupos").OnceAsync<Group>();
            var gruposInscritos = gruposData.Select(grupoData =>
            {
                var grupo = grupoData.Object;
                grupo.IdGrupo = grupoData.Key;
                return grupo;
            }).Where(grupo => grupo.Participants.Any(p => p.CorreoAlumno == currentUserEmail)).ToList();

            // Asignar los grupos en los que está inscrito al ListView
            GruposInscritosListView.ItemsSource = gruposInscritos;
        }

        private async void OnGrupoSeleccionado(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem == null)
                return;

            var grupoSeleccionado = (Group)e.SelectedItem;
            //await Navigation.PushAsync(new PageDetalleGrupo(grupoSeleccionado));

            // Desmarcar el elemento seleccionado en la lista
            ((ListView)sender).SelectedItem = null;
        }
    }
    public class Group
    {
        public string IdGrupo { get; set; }
        public string Name { get; set; }
        public List<Participant> Participants { get; set; } = new List<Participant>();
    }

    public class Participant
    {
        public int IdAlumno { get; set; }
        public string NombreAlumno { get; set; }
        public string CorreoAlumno { get; set; }
    }
}