using SMS.Core.Model;

namespace SMS.Core.Contract.Persistence;

public interface IApplicantAddlQualificationRepository
{
	Task<List<ApplicantAddlQualificationModel>> GetApplicantAddlQualificationsByApplicantId(int ApplicantId);
	Task<ApplicantAddlQualificationModel> GetApplicantAddlQualificationById(int ApplicantAddlQualificationId);
	Task<int> InsertApplicantAddlQualification(ApplicantAddlQualificationModel ApplicantAddlQualification, LogModel logModel);
	Task UpdateApplicantAddlQualification(ApplicantAddlQualificationModel ApplicantAddlQualification, LogModel logModel);
	Task DeleteApplicantAddlQualification(int ApplicantAddlQualificationId, LogModel logModel);
}