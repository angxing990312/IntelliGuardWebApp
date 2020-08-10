using System;
using System.Collections.Generic;

namespace maddweb.Models
{
    public partial class MaddUser
    {
        public MaddUser()
        {
        }

        public MaddUser(int userid)
        {
            Entry = new HashSet<Entry>();
        }

        public MaddUser(int userid, string name, string pw, string path, int contactnumber, string role) : this(userid)
        {
            Role = role;
        }

        public int UserID { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string UserPw { get; set; }
        public string UPhotoPath { get; set; }
        public int UserContact { get; set; }
        public string Role { get; set; }
        public string nric { get; set; }

        public virtual ICollection<Entry> Entry { get; set; }
    }
}
