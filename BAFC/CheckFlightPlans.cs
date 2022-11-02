using BAFC.Objects;
using System.Collections.Generic;
using VatsimAPI;
using System;
using System.Linq;

namespace BAFC
{
    public class CheckFlightPlans
    {
        private static Dictionary<Errors, string> WrongMsg;
        private static List<Airports> Airports = new();
        private static List<AirportRestrictions> AirportRestrictions = new();
        private static List<Sids> Sids = new();
        private static List<Aircraft> Aircrafts;
        private static List<string> Runways;
        public enum Errors
        {
            VFR, MinFL, FixedFL, ForbiddenFL, MaxFL,EvenOdd, NoCorrectSID, WrongDesignation, MinEng, MaxEng, Runway,Elsik,NoAirport,NoSID,NoDesignation
        }
        public static void SetUp(List<Airports> airports, List<AirportRestrictions> airportRestrictions, List<Sids> sids, List<string> runways)
        {
            Airports = airports; AirportRestrictions = airportRestrictions; Sids = sids; Runways = runways;
            Aircrafts = getAircrafts();
        }
        public static bool Update()
        {
            try
            {

            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
#nullable enable
        /// <summary>
        /// Checks all departures from dictionary containg the callsign + flightplan.
        /// </summary>
        /// <param name="departureList"></param>
        /// <exception cref="ArgumentNullException">Returns NullException if Airport, airportRrestriction and Sids are empty</exception>
        /// <returns>A Dictionary with a string containing the Callsign and a List of string containing error msg's</returns>
        public static Dictionary<string, Dictionary<Errors, string>>? CheckPlans(Dictionary<string, FlightPlan> departureList)
        {
            Dictionary<string, Dictionary<Errors,string>> wrongFlightPlans = new();
            /*
            Checks if a flightplan is correct following the Airport Restrictions (destinations) first. 
            Thereafter it will check if the SID is correct
             */

            if (Airports == null)
            {
                throw new ArgumentNullException(nameof(Airports), "Can't be null");
            }
            else if (AirportRestrictions == null)
            {
                throw new ArgumentNullException(nameof(AirportRestrictions), "Can't be null");
            }
            else if (Sids == null)
            {
                throw new ArgumentNullException(nameof(Sids), "Can't be null");
            }
            foreach (var departure in departureList) //check every departure from the airport('s)
            {
                
                WrongMsg = new();
                if (departure.Value.flight_rules == "V") //is vfr
                {
                    WrongMsg.Add(Errors.VFR, "VFR");
                    wrongFlightPlans.Add(departure.Key, WrongMsg);
                    return wrongFlightPlans;
                }
                if (!CorrectAirportRestirction(departure.Value)|| !CorrectSids(departure.Value)) //if they are wrong, return with the errors
                {
                    wrongFlightPlans.Add(departure.Key, WrongMsg);
                }               
                

            }
            if(wrongFlightPlans.Count == 0)
            {
                return null;
            }
            return wrongFlightPlans;
        }
#nullable disable
        static private bool CorrectAirportRestirction(FlightPlan flightPlan)
        {
            bool isCorrect = true;
            string[] route = flightPlan.route.Split(' ');
            foreach (var item in AirportRestrictions) //for every airport in the restriction list
            //FP have been filtered on the controller's airspace, next 'if' statement will filter out departure airports the controller doesn't contol
            {
                if (flightPlan.departure == item.Departure) // check if the departure departs from one of the departure airport restrictions
                {
                    foreach (var restiction in item.restrictions) //for each restriction on that airport
                    {
                        if (restiction.Destination == flightPlan.arrival) //check if teh aircraft flies to one of the restictions 
                        {
                            int fl = int.Parse(flightPlan.altitude) / 100; //Vatsim gives altitude in string, not FL
                            if (restiction.FixedHeight != null) //if the restiction has a fixed height given
                            {
                                if (fl == restiction.FixedHeight)
                                {
                                    WrongMsg.Add(Errors.FixedFL,$"Fixed FL {restiction.FixedHeight} to {restiction.Destination}");
                                    isCorrect = false;
                                }
                            }
                            if (restiction.MaxHeight != null) //if Max height is given
                            {
                                if (fl > restiction.MaxHeight)
                                {
                                    WrongMsg.Add(Errors.MaxFL,$"Maximum FL {restiction.MaxHeight.Value} to {restiction.Destination}");
                                    isCorrect = false;
                                }
                            }
                            if (restiction.SID != null) //check if a SID is given. Check FP if it has this SID
                            {
                                bool containsSid = false;
                                string sidslist = string.Empty;
                                foreach (string sid in restiction.SID)
                                {
                                    if (route[0].Contains(sid))
                                    {
                                        containsSid = true;
                                    }
                                    sidslist += $"{sid} ";
                                }
                                if (!containsSid)
                                {
                                    WrongMsg.Add(Errors.NoCorrectSID,$"Must contain following SID('s) {sidslist}to {restiction.Destination}");
                                    isCorrect = false;
                                }
                            }
                        }
                        if (!isCorrect)
                        {
                            return false; //if the restrition is not correct, it will break and won't check other restrictions
                        }
                    }
                }
            }
            return isCorrect;
        }
        static private bool CorrectSids(FlightPlan flightPlan)
        {
            bool isCorrect = true;
            bool containsAirport = false;
            bool containsSid = false;
            bool containsDesignation = false;
            string sid = flightPlan.route.Split(' ')[0];
            foreach (var airport in Sids)
            {
                if (flightPlan.departure == airport.ICAO) //check if the sid is for the correct airport
                {
                    containsAirport = true;
                    foreach (var airportSid in airport.SIDS.Where(airportSid => sid.Contains(airportSid.Name)))
                    {
                        containsSid = true;
                        foreach (var sidDesignation in airportSid.Designation)
                        {
                            if (sid.Contains(sidDesignation.Designator))
                            {
                                containsDesignation = true;
                                if (sidDesignation.MaxEng != null)
                                {
                                    foreach (var aircraft in Aircrafts.Where(aircraft => aircraft.AircraftType == flightPlan.aircraft && aircraft.EngineCount > sidDesignation.MaxEng))
                                    {
                                        isCorrect = false;
                                        WrongMsg.Add(Errors.MaxEng,$"{sidDesignation.MaxEng} or less engines allowed. This aircraft has {aircraft.EngineCount} engines");
                                    }
                                }
                                if (sidDesignation.MinEng != null)
                                {
                                    foreach (var aircraft in Aircrafts.Where(aircraft => aircraft.AircraftType == flightPlan.aircraft && aircraft.EngineCount < sidDesignation.MinEng))
                                    {
                                        isCorrect = false;
                                        WrongMsg.Add(Errors.MinEng,$"{sidDesignation.MinEng} or more engines are allowed. This aircraft has {aircraft.EngineCount} engines");
                                    }
                                }
                                //check aircraft type if rnav capable and engines
                                //belux
                                bool ContainsRuwnway = false;
                                foreach (var runway in from runway in sidDesignation.Runways from runwayInUse in Runways where runway == runwayInUse select runway)
                                {
                                    ContainsRuwnway = true;
                                }
                                if (!ContainsRuwnway)
                                {
                                    isCorrect = false;
                                    WrongMsg.Add(Errors.Runway,$"{sid}{sidDesignation.Designator} is not used for current runway config");
                                }
                                var currentUTCTime = DateTime.UtcNow;
                                if (airport.ICAO == "EBBR" && Runways.Contains("25R"))
                                {
                                    isCorrect = CheckBelux(flightPlan, isCorrect, sid, airportSid, sidDesignation, currentUTCTime);
                                }
                            }

                            foreach (var airway in airportSid.Airways.Where(airway => flightPlan.route.Contains(airway.airway) || airway.airway == "others"))
                            {
                                if (airway.ForbiddenFL != null)
                                {
                                    foreach (var forbiddenFL in airway.ForbiddenFL.Where(forbiddenFL => flightPlan.altitude == (forbiddenFL * 100).ToString()))
                                    {
                                        isCorrect = false;
                                        WrongMsg.Add(Errors.ForbiddenFL, $"FL{forbiddenFL} is not permitted on the airway {airway.airway}");
                                        break;
                                    }
                                }
                                switch (airway.Direction.ToLower())
                                {
                                    case "odd":
                                    {
                                        if ((int.Parse(flightPlan.altitude) < 41000) && ((int.Parse(flightPlan.altitude) / 1000) % 2 == 0))
                                        {
                                            isCorrect = false;
                                            WrongMsg.Add(Errors.EvenOdd,$"Flights must me odd via airway {airway}");
                                        }

                                        break;
                                    }
                                    case "even":
                                    {
                                        if ((int.Parse(flightPlan.altitude) < 41000) && ((int.Parse(flightPlan.altitude) / 1000) % 2 != 0))
                                        {
                                            isCorrect = false;
                                            WrongMsg.Add(Errors.EvenOdd,$"Flights must me even via airway {airway.airway}");
                                        }

                                        break;
                                    }
                                }
                                if (airway.MinFL != null)
                                {
                                    if (int.Parse(flightPlan.altitude) > airway.MaxFL * 100)
                                    {
                                        isCorrect = false;
                                        WrongMsg.Add(Errors.MinFL,$"Flights may not fly higher than FL{airway.MaxFL} on airway {airway.airway}");
                                    }
                                }

                                if (airway.MinFL == null) continue;
                                if (!(int.Parse(flightPlan.altitude) > airway.MaxFL * 100)) continue;
                                isCorrect = false;
                                WrongMsg.Add(Errors.MaxFL,$"Flights may not fly lower then FL{airway.MinFL} on airway {airway.airway}");
                            }
                        }
                    }
                }
                if (!containsAirport)
                {
                    WrongMsg.Add(Errors.NoAirport,"Airport not found in Sids.Json");
                    isCorrect = false;
                }
                if (!containsSid)
                {
                    WrongMsg.Add(Errors.NoSID,"No SID's found for this airport");
                    isCorrect = false;
                }
                if (!containsDesignation)
                {
                    WrongMsg.Add(Errors.NoDesignation,"No Designation found for this SID");
                }

                return isCorrect;
            }
            return isCorrect;
        }

        private static bool CheckBelux(FlightPlan flightPlan, bool isCorrect, string sid, Sids.SID airportSid, Sids.Designation sidDesignation, DateTime currentUTCTime)
        {
            if ((currentUTCTime.IsDaylightSavingTime() && (currentUTCTime.Hour + 2 >= new DateTime().AddHours(6).Hour &&
                currentUTCTime.Hour + 2 < new DateTime().AddHours(23).Hour)) || (currentUTCTime.Hour + 1 >= new DateTime().AddHours(6).Hour &&
                currentUTCTime.Hour + 1 < new DateTime().AddHours(23).Hour)) //is bentween 6 an d23 Belux LT
            {
                if (sid == "CIV" && !sidDesignation.Designator.Contains("D") && (currentUTCTime.DayOfWeek == DayOfWeek.Saturday ||
                    currentUTCTime.DayOfWeek == DayOfWeek.Sunday)) // is weekend
                {
                    isCorrect = false;
                    WrongMsg.Add(Errors.WrongDesignation, "CIV must contain the D designation on weekends during 6LT-23LT");
                }
                else
                {
                    bool hasD = false;
                    if (sidDesignation.Designator != "D")
                    {
                        foreach (var sidDes in airportSid.Designation)
                        {
                            if (sidDes.Designator.Contains("D")) hasD = true;
                        }
                        if (hasD)
                        {
                            

                            //foreach (var aircraft in Aircrafts)
                            //{
                            //    if (aircraft.AircraftType == aircraft.EngineCount > 3)
                            //    {
                            //        isCorrect = false;
                            //        WrongMsg.Add(Errors.MaxEng, $"heavy aircraft has more than 3 engines and must fly the D SID");
                            //    }
                            //}
                            if(Aircrafts.Find((aircraft) => aircraft.EngineCount > 3)==null)
                            {
                                isCorrect = false;
                                WrongMsg.Add(Errors.MaxEng, $"heavy aircraft has more than 3 engines and must fly the D SID");
                            }

                        }

                    }
                    foreach (var aircraft in Aircrafts.Where(aircraft => aircraft.AircraftType == flightPlan.aircraft && aircraft.EngineCount > 3))
                    {
                        isCorrect = false;
                        WrongMsg.Add(Errors.MaxEng, $"3 or less engines allowed. This aircraft has {aircraft.EngineCount} engines");
                    }
                }
            }
            else if ((currentUTCTime.IsDaylightSavingTime() && currentUTCTime.Hour + 2 < new DateTime().AddHours(6).Hour &&
                 currentUTCTime.Hour + 2 >= new DateTime().AddHours(23).Hour) || (currentUTCTime.Hour + 1 < new DateTime().AddHours(6).Hour &&
                 currentUTCTime.Hour + 1 >= new DateTime().AddHours(23).Hour))//is bentween 6 an d23 Belux LT
            {
                isCorrect = false;
                WrongMsg.Add(Errors.Elsik, "Confirm ELSIK departure with APP/CTR if online");
            }
            else //if outside of 6-23LT
            {
                bool hasZ = false;
                if (sidDesignation.Designator == "Z") return isCorrect;
                foreach (var sidDes in airportSid.Designation.Where(sidDes => sidDes.Designator.Contains("Z")))
                {
                    hasZ = true;
                }
                if (hasZ)
                {
                    isCorrect = false;
                    WrongMsg.Add(Errors.WrongDesignation, "Z sid should be used");
                }
                else if (sidDesignation.Designator != "C")
                {
                    isCorrect = false;
                    WrongMsg.Add(Errors.WrongDesignation, "C should be used");
                }
            }

            return isCorrect;
        }

        public static List<Aircraft> getAircrafts() 
        {
            List<Aircraft> aircraftList = new();
            string aircraftFile = $@"{Properties.Settings.Default.EuroscopeFolder}/DataFiles/ICAO_Aircraft.txt ";
            using System.IO.StreamReader reader = new System.IO.StreamReader(aircraftFile);
            var lastcs = 'A';
            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();

                if (line.StartsWith(';') || line.Contains("----")) continue;
                string[] lineArray = line.Split("\t");
                if (lineArray[0].ToCharArray()[0] != lastcs && lineArray[0].ToCharArray()[0] != lastcs + 1)
                    continue;
                lastcs = lineArray[0].ToCharArray()[0];

                var categorie = lineArray[1][0].ToString() switch
                {
                    "L" => AircraftCategories.Light,
                    "M" => AircraftCategories.Medium,
                    "H" => AircraftCategories.Heavy,
                    "J" => AircraftCategories.Super,
                    _ => (AircraftCategories)5
                };
                try
                {
                    aircraftList.Add(new Aircraft { AircraftType = lineArray[0], AircraftCategorie = categorie, EngineCount = int.Parse(lineArray[1][2].ToString()) });
                }
                catch (Exception)
                {
                    aircraftList.Add(new Aircraft { AircraftType = lineArray[0], AircraftCategorie = categorie, EngineCount = 0 });
                }

            }

            return aircraftList;
        }
    }
}
