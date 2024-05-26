using SMS.Core.Model;

namespace SMS.Core.Contract.Persistence;

public interface IApplicantDisciplineRepository
{
	Task<List<ApplicantDisciplineModel>> GetApplicantDisciplinesByApplicantId(int ApplicantId);
	Task<ApplicantDisciplineModel> GetApplicantDisciplineById(int ApplicantDisciplineId);
	Task<int> InsertApplicantDiscipline(ApplicantDisciplineModel ApplicantDiscipline, LogModel logModel);
	Task UpdateApplicantDiscipline(ApplicantDisciplineModel ApplicantDiscipline, LogModel logModel);
	Task DeleteApplicantDiscipline(int ApplicantDisciplinId, LogModel logModel);
}