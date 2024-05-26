using SMS.Core.Model;

namespace SMS.Core.Contract.Persistence;

public interface ICertificateAuthorityRepository
{
	Task<PaginatedListModel<CertificateAuthorityModel>> GetCertificateAuthoritys(int pageNumber);
	Task<List<CertificateAuthorityModel>> GetDistinctCertificateAuthoritys();
	Task<CertificateAuthorityModel> GetCertificateAuthorityById(int CertificateAuthorityId);
	Task<CertificateAuthorityModel> GetCertificateAuthorityByName(string CertificateAuthorityName);
	Task<int> InsertCertificateAuthority(CertificateAuthorityModel CertificateAuthority, LogModel logModel);
	Task UpdateCertificateAuthority(CertificateAuthorityModel CertificateAuthority, LogModel logModel);
	Task DeleteCertificateAuthority(int CertificateAuthorityId, LogModel logModel);
	Task<List<CertificateAuthorityModel>> Export();
}