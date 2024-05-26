using SMS.Core.Model;

namespace SMS.Core.Contract.Persistence;

public interface IDegreeRepository
{
	Task<PaginatedListModel<DegreeModel>> GetDegrees(int pageNumber);
	Task<List<DegreeModel>> GetDistinctDegrees();
	Task<DegreeModel> GetDegreeById(int DegreeId);
	Task<DegreeModel> GetDegreeByName(string DegreeName);
	Task<int> InsertDegree(DegreeModel Degree, LogModel logModel);
	Task UpdateDegree(DegreeModel Degree, LogModel logModel);
	Task DeleteDegree(int DegreeId, LogModel logModel);
	Task<List<DegreeModel>> Export();
}