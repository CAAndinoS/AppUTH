﻿using Firebase.Auth;
using Firebase.Database;
using Firebase.Storage;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AppUTH.Service
{
    public class UsuarioRepositorio
    {
        static string webapikey = "AIzaSyD8m3Cc9F_TDqXHr7w_MO67hdbuzYXQ_kw";
        FirebaseAuthProvider authProvider;

        FirebaseClient firebaseClient = new FirebaseClient("https://apputh-b2336-default-rtdb.firebaseio.com/");
        FirebaseStorage firebaseStorage = new FirebaseStorage("gs://apputh-b2336.appspot.com");

        public async Task<bool> Save(Models.Usuario user)
        {
            var data = await firebaseClient.Child("usuarios").PostAsync(JsonConvert.SerializeObject(user));
            if (!string.IsNullOrEmpty(data.Key))
            {
                return true;
            }
            return false;
        }

        public UsuarioRepositorio()
        {
            authProvider = new FirebaseAuthProvider(new FirebaseConfig(webapikey));
        }

        public async Task<bool> Register(string email, string nombre, string clave)
        {
            try
            {
                // Crea el usuario en Firebase Authentication
                var token = await authProvider.CreateUserWithEmailAndPasswordAsync(email, clave, nombre);
                if (!string.IsNullOrEmpty(token.FirebaseToken))
                {
                    // Envía el correo de verificación
                    var user = token.FirebaseToken;
                    await authProvider.SendEmailVerificationAsync(user);

                    // Aquí podrías mostrar un mensaje al usuario para informarle que se ha enviado un correo de verificación.
                    // Puedes usar Toast o algún otro mecanismo de notificación en Xamarin Forms Android.

                    return true;
                }

                return false;
            }
            catch (FirebaseAuthException ex)
            {
                // Manejar errores de autenticación aquí, si es necesario.
                // Por ejemplo, si el correo ya está registrado, FirebaseAuthentication.net lanzará una FirebaseAuthException.
                // Puedes mostrar un mensaje al usuario informando sobre el error.
                return false;
            }
        }

        public async Task<string> SignIn(string email, string clave)
        {
            var token = await authProvider.SignInWithEmailAndPasswordAsync(email, clave);
            if (!string.IsNullOrEmpty(token.FirebaseToken))
            {
                return token.FirebaseToken;
            }
            return "";
        }

        public async Task<string> Getrole(string c)
        {
            var usuario = (await firebaseClient.Child("usuarios").OnceAsync<Models.Usuario>()).Select(u => u.Object).ToList().Find(u => u.Correo == c);
            return usuario.Rol;
        }

        public async Task<bool> Resetpassword(string email)
        {
            await authProvider.SendPasswordResetEmailAsync(email);
            return true;
        }
    }
}//SendVerificationEmailAsync
