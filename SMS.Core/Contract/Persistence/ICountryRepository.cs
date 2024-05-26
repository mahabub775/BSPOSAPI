using SMS.Core.Model;

namespace SMS.Core.Contract.Persistence;

public interface ICountryRepository
{
	Task<PaginatedListModel<CountryModel>> GetCountrys(int pageNumber);
	Task<List<CountryModel>> GetDistinctCountrys();
	Task<CountryModel> GetCountryById(int CountryId);
	Task<CountryModel> GetCountryByName(string CountryName);
	Task<int> InsertCountry(CountryModel Country, LogModel logModel);
	Task UpdateCountry(CountryModel Country, LogModel logModel);
	Task DeleteCountry(int CountryId, LogModel logModel);
	Task<List<CountryModel>> Export();
}