using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace WindparkAPIAggregation.Contracts;

public class Turbine
{
    public Guid Id { get; set; }
    public int TurbineNumber { get; set; }
    public double CurrentProduction { get; set; } 
    public double WindSpeed { get; set; }

    [ForeignKey(nameof(WindPark))]
    public Guid WindParkId { get; set; }
    public WindPark WindPark { get; set; }
}