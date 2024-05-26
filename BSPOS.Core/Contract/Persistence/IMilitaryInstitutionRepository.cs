using SMS.Core.Model;

namespace SMS.Core.Contract.Persistence;

public interface IMilitaryInstitutionRepository
{
	Task<PaginatedListModel<MilitaryInstitutionModel>> GetMilitaryInstitutions(int pageNumber);
	Task<List<MilitaryInstitutionModel>> GetDistinctMilitaryInstitutions();
	Task<MilitaryInstitutionModel> GetMilitaryInstitutionById(int MilitaryInstitutionId);
	Task<MilitaryInstitutionModel> GetMilitaryInstitutionByName(string MilitaryInstitutionName);
	Task<int> InsertMilitaryInstitution(MilitaryInstitutionModel MilitaryInstitution, LogModel logModel);
	Task UpdateMilitaryInstitution(MilitaryInstitutionModel MilitaryInstitution, LogModel logModel);
	Task DeleteMilitaryInstitution(int MilitaryInstitutionId, LogModel logModel);
	Task<List<MilitaryInstitutionModel>> Export();
}