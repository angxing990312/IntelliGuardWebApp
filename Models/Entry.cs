using System;
using System.Collections.Generic;

namespace maddweb.Models
{
    public partial class Entry
    {
        public int EntryID { get; set; }
        public int? UserID { get; set; }
        public double Temperature { get; set; }
        public string Photo { get; set; }
        public DateTime EntryTime { get; set; }
        public string Location { get; set; }

        public virtual MaddUser User { get; set; }

		
	}
}
