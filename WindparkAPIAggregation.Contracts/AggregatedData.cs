using System.Collections.Generic;

namespace WindparkAPIAggregation.Contracts;

public class AggregatedData
{
    public List<WindParkAggregated> WindParkAggregatedData { get; set; }
    public DataSource DataSource { get; set; }
}