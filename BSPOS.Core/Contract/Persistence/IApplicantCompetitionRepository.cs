using SMS.Core.Model;

namespace SMS.Core.Contract.Persistence;

public interface IApplicantCompetitionRepository
{
	Task<List<ApplicantCompetitionModel>> GetApplicantCompetitionsByApplicantId(int ApplicantId);
	Task<ApplicantCompetitionModel> GetApplicantCompetitionById(int ApplicantCompetitionId);
	Task<int> InsertApplicantCompetition(ApplicantCompetitionModel ApplicantCompetition, LogModel logModel);
	Task UpdateApplicantCompetition(ApplicantCompetitionModel ApplicantCompetition, LogModel logModel);
	Task DeleteApplicantCompetition(int ApplicantCompetitionId, LogModel logModel);
}