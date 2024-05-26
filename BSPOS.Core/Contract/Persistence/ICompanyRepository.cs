using SMS.Core.Model;

namespace SMS.Core.Contract.Persistence;

public interface ICompanyRepository
{
	Task<PaginatedListModel<CompanyModel>> GetCompanys(int pageNumber);
	Task<List<CompanyModel>> GetCompanysByUnit(int UnitId);
	Task<List<CompanyModel>> GetDistinctCompanys();
	Task<CompanyModel> GetCompanyById(int CompanyId);
	Task<CompanyModel> GetCompanyByName(string CompanyName);
	Task<int> InsertCompany(CompanyModel Company, LogModel logModel);
	Task UpdateCompany(CompanyModel Company, LogModel logModel);
	Task DeleteCompany(int CompanyId, LogModel logModel);
	Task<List<CompanyModel>> Export();
}