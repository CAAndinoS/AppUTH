using System;
using System.Collections.Generic;
using System.Text;

namespace AppUTH.Models
{
    public class AudioEntry
    {
        public string UploadedBy { get; set; } // Nombre del usuario que subió el audio
        public string AudioUrl { get; set; } // URL del audio almacenado en Firebase Storage
        public string Audio { get; set; } // Campo adicional "audio"

    }
}
