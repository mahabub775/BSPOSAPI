using SMS.Core.Model;

namespace SMS.Core.Contract.Persistence;

public interface IApplicantMilitaryTrainingRepository
{
	Task<List<ApplicantMilitaryTrainingModel>> GetApplicantMilitaryTrainingsByApplicantId(int ApplicantId);
	Task<ApplicantMilitaryTrainingModel> GetApplicantMilitaryTrainingById(int ApplicantMilitaryTrainingId);
	Task<int> InsertApplicantMilitaryTraining(ApplicantMilitaryTrainingModel ApplicantMilitaryTraining, LogModel logModel);
	Task UpdateApplicantMilitaryTraining(ApplicantMilitaryTrainingModel ApplicantMilitaryTraining, LogModel logModel);
	Task DeleteApplicantMilitaryTraining(int ApplicantMilitaryTrainingId, LogModel logModel);

}