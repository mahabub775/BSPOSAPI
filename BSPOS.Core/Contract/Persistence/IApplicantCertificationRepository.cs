using SMS.Core.Model;

namespace SMS.Core.Contract.Persistence;

public interface IApplicantCertificationRepository
{
	Task<List<ApplicantCertificationModel>> GetApplicantCertificationsByApplicantId(int ApplicantId);
	Task<ApplicantCertificationModel> GetApplicantCertificationById(int ApplicantCertificationId);
	Task<int> InsertApplicantCertification(ApplicantCertificationModel ApplicantCertification, LogModel logModel);
	Task UpdateApplicantCertification(ApplicantCertificationModel ApplicantCertification, LogModel logModel);
	Task DeleteApplicantCertification(int ApplicantCertificationId, LogModel logModel);
}