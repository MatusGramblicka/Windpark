using System.Threading.Tasks;
using WindparkAPIAggregation.Contracts;

namespace WindparkAPIAggregation.Interface;

public interface IDatabaseOperation
{
    Task<AggregatedData> GetAggregatedDataFromDb();
    Task SaveToDb(WindParkDto windParkData);
}