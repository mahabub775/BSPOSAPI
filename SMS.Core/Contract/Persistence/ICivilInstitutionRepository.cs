using SMS.Core.Model;

namespace SMS.Core.Contract.Persistence;

public interface ICivilInstitutionRepository
{
	Task<PaginatedListModel<CivilInstitutionModel>> GetCivilInstitutions(int pageNumber);
	Task<List<CivilInstitutionModel>> GetDistinctCivilInstitutions();
	Task<List<CivilInstitutionModel>> GetTopCivilEducations();
	Task<CivilInstitutionModel> GetCivilInstitutionById(int CivilInstitutionId);
	Task<CivilInstitutionModel> GetCivilInstitutionByName(string CivilInstitutionName);
	Task<int> InsertCivilInstitution(CivilInstitutionModel CivilInstitution, LogModel logModel);
	Task UpdateCivilInstitution(CivilInstitutionModel CivilInstitution, LogModel logModel);
	Task DeleteCivilInstitution(int CivilInstitutionId, LogModel logModel);
	Task<List<CivilInstitutionModel>> Export();
}