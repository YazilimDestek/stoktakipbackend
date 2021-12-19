using System;
using System.Collections.Generic;
using System.Text;

namespace CinigazStokEntity
{
    public class User : BaseEntity
    {
        public  string Name { get; set; }
        public string Username { get; set; }
        public string Surname { get; set; }
        public string Email{ get; set; }



        public string Password { get; set; }
        public string Unit { get; set; }
        public DateTime LastActiveDate { get; set; }
        public bool IsActive { get; set; }
        public bool? IsAdmin { get; set; }
        
        public bool? ItemAdd { get; set; }
        public bool? ItemEdit { get; set; }
        public bool? ItemDelete { get; set; }


        public bool? CategoryAdd { get; set; }
        public bool? CategoryAEdit{ get; set; }
        public bool? CategoryDelete { get; set; }

        public bool? HistoryAdd { get; set; }
        public bool? HistoryEdit { get; set; }
        public bool? HistoryDelete { get; set; }



    }
}
