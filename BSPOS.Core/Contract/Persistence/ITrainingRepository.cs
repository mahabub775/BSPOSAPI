using SMS.Core.Model;

namespace SMS.Core.Contract.Persistence;

public interface ITrainingRepository
{
	Task<PaginatedListModel<TrainingModel>> GetTrainings(int pageNumber);
	Task<List<TrainingModel>> GetDistinctTrainings();
	Task<TrainingModel> GetTrainingById(int TrainingId);
	Task<TrainingModel> GetTrainingByName(string TrainingName);
	Task<int> InsertTraining(TrainingModel Training, LogModel logModel);
	Task UpdateTraining(TrainingModel Training, LogModel logModel);
	Task DeleteTraining(int TrainingId, LogModel logModel);
	Task<List<TrainingModel>> Export();
}