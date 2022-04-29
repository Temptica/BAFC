using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BAFC.Objects
{
    class AirportRestrictions
    {
        public string Departure { get; set; }
        public Restriction[] restrictions { get; set; }
        

        public class Restriction
        {
            public string Destination { get; set; }
            #nullable enable
            public int? MaxHeight { get; set; }
            public string[]? SID { get; set; }
            public int? FixedHeight { get; set; }
        }

    }
}
