using System.Collections.Generic;
using WindParkAPIAggregation.Contracts.Models;

namespace WindParkAPIAggregation.Contracts;

public class WindParkAggregationPersistor
{
    public List<WindPark> WindParkAggregationData { get; set; } = new();
}