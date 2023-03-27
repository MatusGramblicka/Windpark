using System.Collections.Generic;
using WindparkAPIAggregation.Contracts.Models;

namespace WindparkAPIAggregation.Contracts;

public class WindParkAggregationPersistor
{
    public List<WindPark> WindParkAggregationData { get; set; } = new();
}