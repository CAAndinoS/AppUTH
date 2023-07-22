using System;
using System.Collections.Generic;
using System.Text;

namespace AppUTH.Models
{
    public class PhotoEntry
    {
        public string UploadedBy { get; set; } // Nombre del usuario que subió la foto
        public string FotoBase64 { get; set; } // La foto en formato Base64
        public string Foto { get; set; } // Campo adicional "foto"
    }
}
