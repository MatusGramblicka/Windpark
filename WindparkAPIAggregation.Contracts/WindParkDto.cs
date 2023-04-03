using System.Collections.Generic;

namespace WindParkAPIAggregation.Contracts;

public class WindParkDto
{
    public string Name { get; set; }
    public string Description { get; set; }
    public int Id { get; set; }
    public string Region { get; set; }
    public string Country { get; set; }
    public List<TurbineDto> Turbines { get; set; } = new();
}