﻿using System.Collections.Generic;

namespace WindParkAPIAggregation.Contracts;

public class AggregatedTurbineData
{
    public int TurbineId { get; set; }

    /// <summary>
    /// Tuple WindSpeed to CurrentProduction aggregated
    /// </summary>
    public List<(double, double)> WindSpeedCurrentProductionAggregation { get; set; } = new();
}