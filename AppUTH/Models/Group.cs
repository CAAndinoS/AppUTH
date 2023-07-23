
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
        public List<MultimediaEntry> Multimedia { get; set; } = new List<MultimediaEntry>();
    }
}
