using System.Collections.Generic;
using System.Linq;
using WindParkAPIAggregation.Contracts;
using WindParkAPIAggregation.Contracts.Models;

namespace WindParkAPIAggregation.Core;

public static class Aggregator
{
    public static List<AggregatedTurbineData> Aggregate(IGrouping<int, WindPark> windPark)
    {
        var aggregatedTurbineFlattened = windPark.SelectMany(a => a.Turbines);
        var turbinesGroups = aggregatedTurbineFlattened.GroupBy(w => w.TurbineNumber);

        var aggregatedTurbinesData = new List<AggregatedTurbineData>();

        foreach (var turbines in turbinesGroups)
        {
            var aggregatedTurbineData = new AggregatedTurbineData
            {
                TurbineId = turbines.Key,
                WindSpeedCurrentProductionAggregation =
                    turbines.Select(t => (t.WindSpeed, t.CurrentProduction)).ToList()
            };

            aggregatedTurbinesData.Add(aggregatedTurbineData);
        }

        return aggregatedTurbinesData;
    }
}