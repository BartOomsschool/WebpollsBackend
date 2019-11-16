using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace OpdrachtAPI.Models
{
    public class Poll
    {
        public long PollID { get; set; }
        public string Naam { get; set; }
        [NotMapped]
        public bool Gestemd { get; set; }

    }
}
