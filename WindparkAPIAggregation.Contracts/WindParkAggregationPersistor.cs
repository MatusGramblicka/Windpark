using System.Collections.Generic;

namespace WindparkAPIAggregation.Contracts
{
    public class WindParkAggregationPersistor
    {
        public List<WindParkAggregationData> WindParkAggregationData { get; set; } =
            new List<WindParkAggregationData>();
    }
}