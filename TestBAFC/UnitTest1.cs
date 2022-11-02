using BAFC;
using BAFC.Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using VatsimAPI;

namespace TestBAFC
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            WebClient client = new();

            CheckFlightPlans.SetUp(JsonConvert.DeserializeObject<List<Airports>>(new StreamReader(@"../../../../BAFC\Properties\Airports.json").ReadToEndAsync().Result),
                JsonConvert.DeserializeObject<List<AirportRestrictions>>(new StreamReader(@"../../../../BAFC\Properties\AirportRestrictions.json").ReadToEndAsync().Result),
                JsonConvert.DeserializeObject<List<Sids>>(new StreamReader(@"../../../..\BAFC\Properties\Sids.Json").ReadToEndAsync().Result), new() { "25L", "25R" });

            var vatsimObject = JsonConvert.DeserializeObject<VatsimObject>(client.DownloadString("https://data.vatsim.net/v3/vatsim-data.json"));

            Dictionary<string, FlightPlan> DepartureList = new();
            for (int i = 0; i < 100; i++)
            {
                DepartureList.Add($"BEL{i}H", new() { altitude = "30000", arrival = "EGLL", departure = "EBBR", route = "CIV5C CIV DCT KOK", aircraft="B737" });
            }           
            Stopwatch stopwatch = new();
            Console.WriteLine(DateTime.Now.Millisecond);
            stopwatch.Start();
            var result = CheckFlightPlans.CheckPlans(DepartureList);
            stopwatch.Stop();
            Console.WriteLine(DateTime.Now.Millisecond);
            //foreach (var mistake in result)
            //{
            //    foreach (var mistakemsg in mistake.Value)
            //    {
            //        Console.WriteLine($"{mistake.Key}: {mistakemsg}.");
            //    }
            //}
            Console.WriteLine($"Checking took {stopwatch.ElapsedMilliseconds}ms for {DepartureList.Count} departures. {result.Count} mistakes found");

        }
        [TestMethod]
        public void TestAirplaneList()
        {          
            
            foreach (var item in CheckFlightPlans.getAircrafts())
            {
                Console.WriteLine($"{item.AircraftType} {item.EngineCount} {item.AircraftCategorie}");
            }    
        }
    }
}
