namespace WindparkAPIAggregation.Contracts
{
    public class AggregatedTurbine
    {
        public int TurbineId { get; set; }
        //public List<double> CurrentProduction { get; set; } = new List<double>();
        //public List<double> windspeed { get; set; } = new List<double>();

        public double CurrentProduction { get; set; } 
        public double WindSpeed { get; set; } 
    }
}