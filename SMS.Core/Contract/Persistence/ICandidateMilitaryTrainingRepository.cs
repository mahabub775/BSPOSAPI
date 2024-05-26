using SMS.Core.Model;

namespace SMS.Core.Contract.Persistence;

public interface ICandidateMilitaryTrainingRepository
{
	Task<List<CandidateMilitaryTrainingModel>> GetCandidateMilitaryTrainingsByCandidateId(int CandidateId);
	Task<CandidateMilitaryTrainingModel> GetCandidateMilitaryTrainingById(int CandidateMilitaryTrainingId);
	Task<int> InsertCandidateMilitaryTraining(CandidateMilitaryTrainingModel CandidateMilitaryTraining, LogModel logModel);
	Task UpdateCandidateMilitaryTraining(CandidateMilitaryTrainingModel CandidateMilitaryTraining, LogModel logModel);
	Task DeleteCandidateMilitaryTraining(int CandidateMilitaryTrainingId, LogModel logModel);

}