using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace OpdrachtAPI.Models
{
    public class PollUser
    {
        public long PollUserID { get; set; }
        public long PollID { get; set; }
        public bool gestemd { get; set; }
        public bool admin { get; set; }
        public long UserID { get; set; }
        [ForeignKey("PollID")]
        public Poll Poll { get; set; }
        [ForeignKey("UserID")]
        public User User { get; set; }
    }
}
