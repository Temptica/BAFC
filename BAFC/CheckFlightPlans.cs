using BAFC.Objects;
using System.Collections.Generic;
using VatsimAPI;
using System;

namespace BAFC
{
    public class CheckFlightPlans
    {
        static List<string> WrongMsg;
        private static List<Airports> Airports = new();
        private static List<AirportRestrictions> AirportRestrictions = new();
        private static List<Sids> Sids = new();
        public static void SetUp(List<Airports> airports, List<AirportRestrictions> airportRestrictions, List<Sids> sids)
        {
            Airports = airports; AirportRestrictions = airportRestrictions; Sids = sids;
        }
#nullable enable
        /// <summary>
        /// Checks all departures from dictionary containg the callsign + flightplan.
        /// </summary>
        /// <param name="departureList"></param>
        /// <exception cref="ArgumentNullException">Returns NullException if Airport, airportRrestriction and Sids are empty</exception>
        /// <returns>A Dictionary with a string containing the Callsign and a List of string containing error msg's</returns>
        public static Dictionary<string, List<string>>? CheckPlans(Dictionary<string, FlightPlan> departureList) 
        {
            Dictionary<string, List<string>> wrongFlightPlans= new();
            /*
            Checks if a flightplan is correct following the Airport Restrictions (destinations) first. 
            Thereafter it will check if the SID is correct
             */
            if(Airports == null)
            {
                throw new ArgumentNullException(nameof(Airports), "Can't be null");
            }
            else if (AirportRestrictions == null)
            {
                throw new ArgumentNullException(nameof(AirportRestrictions), "Can't be null");
            }
            else if(Sids == null)
            {
                throw new ArgumentNullException(nameof(Sids), "Can't be null");
            }
            foreach (var departure in departureList) //check evey departure from the airport('s)
            {
                WrongMsg = new();
                if (departure.Value.flight_rules == "V") break;
                if (!CorrectAirportRestirction(departure.Value))
                {
                    wrongFlightPlans.Add(departure.Key, WrongMsg);
                }
                    
            }


            if (wrongFlightPlans == null)
            {
                return null;
            }
            
            return wrongFlightPlans;
        }

        static private bool CorrectAirportRestirction(FlightPlan flightPlan)
        {
            bool isCorrect = true;
            string[] route = flightPlan.route.Split(' ');
            foreach (var item in AirportRestrictions) //for every airport in the restriction list
            //FP have been filtered on the controller's airspace, next 'if' statement will filter out departure airports the controller doesn't contol
            {
                if(flightPlan.departure == item.Departure) // check if the departure departs from one of the departure airport restrictions
                {
                    foreach (var restiction in item.restrictions) //for each restriction on that airport
                    {
                        if(restiction.Destination == flightPlan.arrival) //check if teh aircraft flies to one of the restictions 
                        {
                            int fl = int.Parse(flightPlan.altitude) / 100; //Vatsim gives altitude in string, not FL
                            if (restiction.FixedHeight != null) //if the restiction has a fixed height given
                            {
                                if(fl == restiction.FixedHeight) 
                                {
                                    WrongMsg.Add($"Fixed FL {restiction.FixedHeight} to {restiction.Destination}");
                                    isCorrect = false;
                                }
                            }
                            if (restiction.MaxHeight != null) //if Max height is given
                            {
                                if (fl > restiction.MaxHeight)
                                {
#pragma warning disable CS8629 // Nullable value type may be null.
                                    WrongMsg.Add($"Maximum FL {restiction.MaxHeight.Value} to {restiction.Destination}");
#pragma warning restore CS8629 // Nullable value type may be null.
                                    isCorrect = false;
                                }
                            }
                            if(restiction.SID != null) //check if a SID is given. Check FP if it has this SID
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
                                    WrongMsg.Add($"Must contain following SID('s) {sidslist}to {restiction.Destination}");
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
    }
}
