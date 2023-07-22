using System;
using System.Collections.Generic;
using System.Text;

namespace AppUTH.Models
{
    public class PhotoEntry
    {
        public string UploadedBy { get; set; } // Nombre del usuario que subió la foto
        public string FotoUrl { get; set; } // URL de la foto almacenada en Firebase Storage
        public string Foto { get; set; } // Campo adicional "foto"

    }
}
