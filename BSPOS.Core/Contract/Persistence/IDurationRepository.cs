using SMS.Core.Model;

namespace SMS.Core.Contract.Persistence;

public interface IDurationRepository
{
	Task<PaginatedListModel<DurationModel>> GetDurations(int pageNumber);
	Task<List<DurationModel>> GetDistinctDurations();
	Task<DurationModel> GetDurationById(int DurationId);
	Task<DurationModel> GetDurationByName(string DurationName);
	Task<int> InsertDuration(DurationModel Duration, LogModel logModel);
	Task UpdateDuration(DurationModel Duration, LogModel logModel);
	Task DeleteDuration(int DurationId, LogModel logModel);
	Task<List<DurationModel>> Export();
}