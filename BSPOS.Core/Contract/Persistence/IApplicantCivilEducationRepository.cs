using SMS.Core.Model;

namespace SMS.Core.Contract.Persistence;

public interface IApplicantCivilEducationRepository
{
	Task<List<ApplicantCivilEducationModel>> GetApplicantCivilEducationsByApplicantId(int ApplicantId);
	Task<ApplicantCivilEducationModel> GetApplicantCivilEducationById(int ApplicantCivilEducationId);
	Task<int> InsertApplicantCivilEducation(ApplicantCivilEducationModel ApplicantCivilEducation, LogModel logModel);
	Task UpdateApplicantCivilEducation(ApplicantCivilEducationModel ApplicantCivilEducation, LogModel logModel);
	Task DeleteApplicantCivilEducation(int ApplicantCivilEducationId, LogModel logModel);
}