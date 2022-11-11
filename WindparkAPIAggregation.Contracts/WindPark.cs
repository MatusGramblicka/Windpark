using System.Collections.Generic;

namespace WindparkAPIAggregation.Contracts
{
    public class WindPark
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int Id { get; set; }
        public string Region { get; set; }
        public string Country { get; set; }
        public List<Turbine> Turbines { get; set; } = new List<Turbine>();
    }
}