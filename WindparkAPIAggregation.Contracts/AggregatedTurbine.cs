using System.Collections.Generic;

namespace WindparkAPIAggregation.Contracts
{
    public class AggregatedTurbine
    {
        public int Id { get; set; }
        public List<double> CurrentProduction { get; set; } = new List<double>();
        public List<double> windspeed { get; set; } = new List<double>();
    }
}