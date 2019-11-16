using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace OpdrachtAPI.Models
{
    public class VriendUser
    {
        public long VriendUserID { get; set; }
        public long VriendID { get; set; }
        public long UserID { get; set; }

        public bool Status { get; set; }

        [ForeignKey("VriendID")]
        public Vriend Vriend { get; set; }

        [ForeignKey("UserID")]
        public User User { get; set; }
    }
}
