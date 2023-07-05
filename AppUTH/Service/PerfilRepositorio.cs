using AppUTH.Models;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Database.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppUTH.Service
{
    public class PerfilRepositorio
    {
        public string Clientekey { get; set; }
        static string webapikey = "AIzaSyD8m3Cc9F_TDqXHr7w_MO67hdbuzYXQ_kw";
        FirebaseAuthProvider authProvider;
        public static FirebaseClient firebaseperfil = new FirebaseClient("https://apputh-b2336-default-rtdb.firebaseio.com/");

        public async Task UpdatePerfil(Alumno perfil)
        {
            /*var toUpdatePerfil = (await firebaseperfil
                .Child("clientes")
                .OnceAsync<Cliente>()).Where(a => a.Object.Correo == perfil1.Correo).FirstOrDefault();*/
            if (Clientekey != null)
            {
                await firebaseperfil
                .Child("alumnos")
                .Child(Clientekey)
                .PutAsync(perfil);
            }
        }

        public async Task<Alumno> ObtenerAlumno(string correo)
        {
            /*var cliente = (await firebaseperfil
                            .Child("clientes")
                            .OnceAsync<Cliente>()).Where(a => a.Object.Correo == correo).FirstOrDefault();

            return cliente.Object;*/

            var clientes = (await firebaseperfil
            .Child("alumnos")
                            .OnceAsync<Alumno>()).Select(c => {
                                if (c.Object.Correo == correo)
                                {
                                    Clientekey = c.Key;
                                }
                                return c.Object;
                            }).ToList();

            Alumno cl = null;

            foreach (var cliente in clientes)
            {
                if (cliente.Correo == correo)
                {
                    cl = cliente;
                }
            }
            return cl;
        }

        public async Task Guardar_Alumno(Alumno c)
        {
            await firebaseperfil
                .Child("alumnos")
                .PostAsync(c);
        }
        public async Task<int> ObtenerID_Alumno()
        {
            var id = (await firebaseperfil
            .Child("clientes")
                            .OnceAsync<Alumno>()).Select(c => {
                                Clientekey = c.Key;
                                return c.Object;
                            }).ToList().ToArray().Length;

            return id + 1;
        }

        public PerfilRepositorio()
        {
            authProvider = new FirebaseAuthProvider(new FirebaseConfig(webapikey));
        }
    }
}
