using System;
using System.Collections.Generic;
using System.Text;

namespace AppUTH.Models
{
    public class VideoEntry
    {
        public string UploadedBy { get; set; } // Nombre del usuario que subió el video
        public string VideoUrl { get; set; } // URL del video almacenado en Firebase Storage
        public string Video { get; set; } // Campo adicional "video"


    }
}
