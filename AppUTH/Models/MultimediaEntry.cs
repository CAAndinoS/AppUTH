using System;
using System.Collections.Generic;
using System.Text;

namespace AppUTH.Models
{
    public class MultimediaEntry
    {
        public string UploadedBy { get; set; } // Nombre del usuario que subió el archivo multimedia
        public string FileUrl { get; set; } // URL del archivo almacenado en Firebase Storage
        public string Type { get; set; } // Tipo de archivo multimedia (foto, video, audio)
    }
}
