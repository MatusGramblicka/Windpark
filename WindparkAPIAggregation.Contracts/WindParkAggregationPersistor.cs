using System.Collections.Generic;

namespace WindparkAPIAggregation.Contracts;

public class WindParkAggregationPersistor
{
    public List<WindPark> WindParkAggregationData { get; set; } = new();
}