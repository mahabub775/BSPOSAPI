using SMS.Core.Model;

namespace SMS.Core.Contract.Persistence;

public interface IApplicantRoleMappingRepository
{
	Task<PaginatedListModel<ApplicantRoleMappingModel>> GetApplicantRoleMappings(int pageNumber,int BrigadeID, int UnitID, int CompanyID, int PlatoonID);
	Task<ApplicantRoleMappingModel> GetApplicantRoleMappingById(int ApplicantRoleMappingId);
	Task<int> InsertApplicantRoleMapping(ApplicantRoleMappingModel ApplicantRoleMapping, LogModel logModel);
	Task UpdateApplicantRoleMapping(ApplicantRoleMappingModel ApplicantRoleMapping, LogModel logModel);
	Task DeleteApplicantRoleMapping(int ApplicantRoleMappingId, LogModel logModel);
	Task<List<ApplicantRoleMappingModel>> Export(int BrigadeID, int UnitId, int CompanyId, int PlatoonId);
}