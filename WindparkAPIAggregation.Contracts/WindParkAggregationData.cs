using System.Collections.Generic;

namespace WindparkAPIAggregation.Contracts
{
    public class WindParkAggregationData
    {
        public int WindParkId { get; set; }
        public List<AggregatedTurbine> AggregatedTurbine { get; set; } = new List<AggregatedTurbine>();
    }
}