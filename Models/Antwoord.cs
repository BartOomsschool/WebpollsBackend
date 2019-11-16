using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace OpdrachtAPI.Models
{
    public class Antwoord
    {
        public long AntwoordID { get; set; }
        public string AntwoordText { get; set; }
        public long PollID { get; set; }
        [NotMapped]
        public long AantalGestemd { get; set; }
    }
}
