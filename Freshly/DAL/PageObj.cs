using System;
using System.Collections.Generic;

namespace Freshly.Identity {

    public class Page<T>
    {
        public List<T> Items { get; set; }
        public int Number { get; set; }
        public int Size { get; set; }
        public string filterQ { get; set; }
        public int TotalPages { get; set; }

        public Page(List<T> lst, int PNo, int PSize, int TotP, string fQ)
        {
            Items = lst;
            Number = PNo;
            Size = PSize;
            TotalPages = TotP;
            filterQ = fQ;
        }
    }

 }
