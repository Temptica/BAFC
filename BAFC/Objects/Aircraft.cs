namespace BAFC.Objects
{
    public enum AircraftCategories { Light, Medium, Heavy, Super }
    public class Aircraft
    {
        
        public string AircraftType { get; set; }
        public AircraftCategories AircraftCategorie { get; set; }
        public int EngineCount { get; set; }

    }
}