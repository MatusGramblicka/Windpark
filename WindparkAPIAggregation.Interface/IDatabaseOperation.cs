using System.Threading.Tasks;
using WindParkAPIAggregation.Contracts;

namespace WindParkAPIAggregation.Interface;

public interface IDatabaseOperation
{
    Task<AggregatedData> GetAggregatedDataFromDb();
    Task SaveToDb(WindParkDto windParkData);
}