using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BAFC.Objects
{
    public class Sids
    {
        public string ICAO { get; set; }
        public List<SID> SIDS { get; set; }

        public class Designation
        {
            public string Designator { get; set; }
            public List<string> Runways { get; set; }
            public bool ReqRNAV { get; set; }
            public int? MaxEng { get; set; }
            public int? MinEng { get; set; }
        }

        public class Airway
        {
            public string airway { get; set; }
            
            public string Direction { get; set; }
            public List<int> ForbiddenFL { get; set; }
            public int? MaxFL { get; set; }
            public int? MinFL { get; set; }
        }

        public class SID
        {
            public string Name { get; set; }
            public List<Designation> Designation { get; set; }
            public List<Airway> Airways { get; set; }
        }
    }
}
