using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace WindparkAPIAggregation.Contracts.Models;

public class WindPark
{
    [Column("WindParkId")] 
    public Guid Id { get; set; }
    public int WindParkNumber { get; set; }
    public DateTime DateAdded { get; set; }
    public ICollection<Turbine> Turbines { get; set; }
}