﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BAFC.Objects
{
    public class Airports
    {
        public string ICAO { get; set; }
        public int Elevation { get; set; }
        public float Latitude { get; set; }
        public float Longitude { get; set; }
        public string[] Runways { get; set; }

    }
}
