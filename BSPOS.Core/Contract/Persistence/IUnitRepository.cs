using SMS.Core.Model;

namespace SMS.Core.Contract.Persistence;

public interface IUnitRepository
{
	Task<PaginatedListModel<UnitModel>> GetUnits(int pageNumber);
	Task<List<UnitModel>> GetUnitsByBrigade(int BrigadeId);
	Task<List<UnitModel>> GetDistinctUnits();
	Task<UnitModel> GetUnitById(int UnitId);
	Task<UnitModel> GetUnitByName(string UnitName);
	Task<int> InsertUnit(UnitModel Unit, LogModel logModel);
	Task UpdateUnit(UnitModel Unit, LogModel logModel);
	Task DeleteUnit(int UnitId, LogModel logModel);
	Task<List<UnitModel>> Export();
}