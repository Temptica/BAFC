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

            CheckFlightPlans.SetUp(JsonConvert.DeserializeObject<List<Airports>>(new StreamReader(@"D:\Codes\BAFC\BAFC\Properties\Airports.json").ReadToEndAsync().Result),
                JsonConvert.DeserializeObject<List<AirportRestrictions>>(new StreamReader(@"D:\Codes\BAFC\BAFC\Properties\AirportRestrictions.json").ReadToEndAsync().Result),
                JsonConvert.DeserializeObject<List<Sids>>(new StreamReader(@"D:\Codes\BAFC\BAFC\Properties\Sids.Json").ReadToEndAsync().Result));

            var vatsimObject = JsonConvert.DeserializeObject<VatsimObject>(client.DownloadString("https://data.vatsim.net/v3/vatsim-data.json"));

            Dictionary<string, FlightPlan> DepartureList = new();
            DepartureList.Add("BEL52H", new() { altitude = "30000", arrival = "EGLL", departure = "EBBR", route = "CIV5C CIV DCT KOK" });
            Stopwatch stopwatch = new();
            stopwatch.Start();
            var result = CheckFlightPlans.CheckPlans(DepartureList);
            stopwatch.Stop();
            int mistakes = 0;
            foreach (var mistake in result)
            {
                foreach (var mistakemsg in mistake.Value)
                {
                    Console.WriteLine($"{mistake.Key}: {mistakemsg}.");
                    mistakes++;
                }
            }
            Console.WriteLine($"Checking took {stopwatch.ElapsedMilliseconds}ms for {DepartureList.Count} departures. {mistakes} mistakes found");

        }
    }
}
