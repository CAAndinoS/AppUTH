
using System;
using System.Collections.Generic;
using System.Text;

namespace AppUTH.Models
{
    public class Group
    {
        public string IdGrupo { get; set; }
        public string Name { get; set; }
        public List<Participant> Participants { get; set; } = new List<Participant>();
        public List<PhotoEntry> Photos { get; set; } = new List<PhotoEntry>(); // Lista de fotos subidas por los usuarios
        public List<VideoEntry> Videos { get; set; } = new List<VideoEntry>(); // Lista de videos subidos por los usuarios

    }
}
