using System;
using VatsimAPI;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net;
using System.Drawing;
using System.Resources;
using System.Collections.Generic;
using System.IO;
using BAFC.Objects;

namespace BAFC
{
    class Program
    {
        internal static VatsimObject VatsimObject = new();
        internal static WebClient client = new();
        internal static List<Airports>Airports = new();
        internal static List<Positions> Positions = new();
        internal static List<AirportRestrictions> AirportRestrictions = new();
        internal static List<Sids> Sids = new();
        static Positions CurrentPosition;

        public static object CheckFLightPlans { get; private set; }

        static void Main(string[] args)
        {
            try
            {
                DateTime start = DateTime.Now;

                InitialSetUp();
                DateTime stop = DateTime.Now;
                Console.WriteLine($"Initial setup succesfully completed in {(start-stop).Duration().TotalMilliseconds}ms. Starting check loop");
                
                CheckLoop();
            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine(e.ParamName);
                return;
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private static void InitialSetUp()
        {
            Airports = JsonConvert.DeserializeObject<List<Airports>>(new StreamReader("../../../Properties/Airports.json").ReadToEndAsync().Result); //get Json file from Folder, read and convert to a string. Parse it to the Object
            Positions = JsonConvert.DeserializeObject< List<Positions>>(new StreamReader("../../../Properties/Positions.json").ReadToEndAsync().Result);
            AirportRestrictions = JsonConvert.DeserializeObject< List<AirportRestrictions>>(new StreamReader("../../../Properties/AirportRestrictions.json").ReadToEndAsync().Result);
            Sids = JsonConvert.DeserializeObject<List<Sids>>(new StreamReader("../../../Properties/Sids.json").ReadToEndAsync().Result);
            getAPi();
            GetCurrentPosition();
        }

        private static void GetCurrentPosition()
        {
            if(Properties.Settings.Default.CID == 0)
            {
                FirstTimeSetUp();
            }
            foreach (var controller in VatsimObject.controllers)
            {
                if(controller.cid == Properties.Settings.Default.CID)
                {
                    foreach (var position in Positions)
                    {
                        if (position.Callsign == controller.callsign)
                        {
                            CurrentPosition = position;
                            return;
                        }
                    }
                }
            }
            throw new ArgumentNullException("Controller not found. Is your CID correct and are you online on an active position?");
        }

        private static void FirstTimeSetUp()
        {
            Console.WriteLine("First Setup\n________________________\n");
            do
            {
                Console.WriteLine("Please write your CID below:");
                string input = Console.ReadLine();
                try
                {
                    int.Parse(input);
                }
                catch (ArgumentNullException)
                {
                    Console.WriteLine("CID can't be empty.");
                }
                catch (FormatException)
                {
                    Console.WriteLine("CID may not include any letters");
                }
                catch (OverflowException)
                {
                    Console.WriteLine("CID seems to be invalid");
                }
                if(input.Length != 7 && input.Length != 6)
                {
                    Console.WriteLine("CID must be 6-7 numbers long");
                }
                else
                {
                    Properties.Settings.Default.CID = int.Parse(input);
                    break;
                }
            }
            while (true);
            

        }
        private static void CheckLoop()
        {
            Dictionary<string, FlightPlan> departureList = new();
            while (true)
            {
                 = new List<Pilot>();
                getAPi();
                foreach (var pilot in VatsimObject.pilots)
                {
                    foreach (var depAirport in CurrentPosition.Airports)
                    {
                        if(pilot.flight_plan.departure == depAirport)
                        {
                            departureList.Add(pilot.callsign,pilot.flight_plan);
                        }
                    }
                }

                foreach (var departure in DepartureList)
                {
                    CheckFLightPlans.
                }
                Task.Delay(60000);
            };            
        }

        private static void getAPi()
        {
            VatsimObject = JsonConvert.DeserializeObject<VatsimObject>(client.DownloadString("https://data.vatsim.net/v3/vatsim-data.json"));
        }
    }
}
