using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VatsimAPI;

namespace BAFC
{
    public class CheckFlightPlans
    {
        static List<string> WrongMsg;

        #nullable enable

        public static Dictionary<string, FlightPlan>? CheckPlans(Dictionary<string, FlightPlan> departureList)
        {
            /*
            Checks if a flightplan is correct following the Airport Restrictions (destinations) first. 
            Thereafter it will check if the SID is correct
             */
            bool IsCorrect = true;
            WrongMsg = new();
            foreach (var departure in departureList)
            {
                if (departure.Value.flight_rules == "V") break;
                if (!CorrectAirportRestirction(departure.Value))
                {
                    IsCorrect = false;
                }
                    
            }

            
            
            
            return null;
        }

        static private bool CorrectAirportRestirction(FlightPlan flightPlan)
        {
            foreach (var item in Program.AirportRestrictions)
            {
                if(flightPlan.departure == item.Departure)
                {
                    foreach (var restiction in item.restrictions)
                    {
                        if(restiction.Destination == flightPlan.arrival
                    }
                }
            }
            return true;
        }
    }
}
