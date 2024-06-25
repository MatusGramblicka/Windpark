using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace WindParkAPIAggregation.Contracts.Models;

public class SupportPerson
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public Turbine Turbine { get; set; }
    [ForeignKey(nameof(Turbine))]
    public Guid TurbineId { get; set; }
}