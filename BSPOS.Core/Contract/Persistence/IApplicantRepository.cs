using SMS.Core.Model;

namespace SMS.Core.Contract.Persistence;

public interface IApplicantRepository
{
	Task<PaginatedListModel<ApplicantModel>> GetApplicants(int pageNumber, int BrigadeID, int UnitId, int CompanyId, int PlatoonId, int TradeId, int RankId , string SoldierUserId, string ArmyNo, string Name );
	Task<List<ApplicantModel>> GetGroupReport(int UnitId, int CompanyId, int PlatoonId, int TradeId, int RankId );
	Task<ApplicantModel> GetApplicantById(int ApplicantId);
	Task<ApplicantModel> GetApplicantByUserId(string UserId);
	Task<ApplicantModel> GetApplicantByName(string Name);
	Task DeleteApplicant(int ApplicantId, LogModel logModel);
	Task<int> InsertApplicant(ApplicantModel Applicant, LogModel logModel);
	Task UpdateApplicant(ApplicantModel Applicant, LogModel logModel);
	Task<List<ApplicantModel>> Export(int BrigadeID, int UnitId, int CompanyId, int PlatoonId, int TradeId, int RankId, string SoldierUserId, string ArmyNo, string Name);
}