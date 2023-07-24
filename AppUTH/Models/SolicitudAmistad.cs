using System;
using System.Collections.Generic;
using System.Text;

namespace AppUTH.Models
{
    public class SolicitudAmistad
    {
        public string Remitente { get; set; }

        public SolicitudAmistad(string remitente)
        {
            Remitente = remitente;
        }
    }
}
