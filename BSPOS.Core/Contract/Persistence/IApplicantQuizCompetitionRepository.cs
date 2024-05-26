using SMS.Core.Model;

namespace SMS.Core.Contract.Persistence;

public interface IApplicantQuizCompetitionRepository
{
	Task<List<ApplicantQuizCompetitionModel>> GetApplicantQuizCompetitionsByApplicantId(int ApplicantId);
	Task<ApplicantQuizCompetitionModel> GetApplicantQuizCompetitionById(int ApplicantQuizCompetitionId);
	Task<int> InsertApplicantQuizCompetition(ApplicantQuizCompetitionModel ApplicantQuizCompetition, LogModel logModel);
	Task UpdateApplicantQuizCompetition(ApplicantQuizCompetitionModel ApplicantQuizCompetition, LogModel logModel);
	Task DeleteApplicantQuizCompetition(int ApplicantQuizCompetitionId, LogModel logModel);
}