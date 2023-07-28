using System;
using System.Collections.Generic;
using System.Text;

namespace AppUTH.Models
{
    public class Alumno
    {
        public int IdAlumno { get; set; }
        public string Correo { get; set; }
        public string NombreAlumno { get; set; }
        public string Telefono { get; set; }
        public string Direccion { get; set; }
        public string Foto { get; set; }
        // Lista de amigos directamente en el perfil del Alumno
        public List<Amigo> ListaAmigos { get; set; } = new List<Amigo>();
    }
}
