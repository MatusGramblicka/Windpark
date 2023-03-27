using System.Collections.Generic;

namespace WindparkAPIAggregation.Contracts;

public class WindParkAggregated
{
    public int WindParkId { get; set; }
    public List<AggregatedTurbineData> AggregatedTurbineData { get; set; } = new List<AggregatedTurbineData>();
}