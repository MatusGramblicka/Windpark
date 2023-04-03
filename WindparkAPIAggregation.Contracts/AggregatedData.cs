using System.Collections.Generic;

namespace WindParkAPIAggregation.Contracts;

public class AggregatedData
{
    public List<WindParkAggregated> WindParkAggregatedData { get; set; }
    public DataSource DataSource { get; set; }
}