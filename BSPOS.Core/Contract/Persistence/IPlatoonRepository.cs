using SMS.Core.Model;

namespace SMS.Core.Contract.Persistence;

public interface IPlatoonRepository
{
	Task<PaginatedListModel<PlatoonModel>> GetPlatoons(int pageNumber);
	Task<List<PlatoonModel>> GetPlatoonsByCompany(int CompanyId);
	Task<List<PlatoonModel>> GetDistinctPlatoons();
	Task<PlatoonModel> GetPlatoonById(int PlatoonId);
	Task<PlatoonModel> GetPlatoonByName(string PlatoonName);
	Task<int> InsertPlatoon(PlatoonModel Platoon, LogModel logModel);
	Task UpdatePlatoon(PlatoonModel Platoon, LogModel logModel);
	Task DeletePlatoon(int PlatoonId, LogModel logModel);
	Task<List<PlatoonModel>> Export();
}