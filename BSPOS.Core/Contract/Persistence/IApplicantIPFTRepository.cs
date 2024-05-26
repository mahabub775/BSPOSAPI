using SMS.Core.Model;

namespace SMS.Core.Contract.Persistence;

public interface IApplicantIPFTRepository
{
	Task<List<ApplicantIPFTModel>> GetApplicantIPFTsByApplicantId(int ApplicantId);
	Task<ApplicantIPFTModel> GetApplicantIPFTById(int ApplicantIPFTId);
	Task<int> InsertApplicantIPFT(ApplicantIPFTModel ApplicantIPFT, LogModel logModel);
	Task UpdateApplicantIPFT(ApplicantIPFTModel ApplicantIPFT, LogModel logModel);
	Task DeleteApplicantIPFT(int ApplicantIPFTId, LogModel logModel);
}