using System;
using System.Collections.Generic;

namespace Freshly.Identity.Models {

    public class UsersPage
    {
        public List<UsersListItem> Items { get; set; }
        public int Number { get; set; }
        public int Size { get; set; }
        public string FilterQ { get; set; }
        public int TotalPages { get; set; }

        public UsersPage(List<UsersListItem> lst, int PNo, int PSize, int TotP, string fQ)
        {
            Items = lst;
            Number = PNo;
            Size = PSize;
            TotalPages = TotP;
            FilterQ = fQ;
        }
    }

 }
