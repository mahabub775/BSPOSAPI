using SMS.Core.Model;

namespace SMS.Core.Contract.Persistence;

public interface ICertificateRepository
{
	Task<PaginatedListModel<CertificateModel>> GetCertificates(int pageNumber);
	Task<List<CertificateModel>> GetDistinctCertificates();
	Task<CertificateModel> GetCertificateById(int CertificateId);
	Task<CertificateModel> GetCertificateByName(string CertificateName);
	Task<int> InsertCertificate(CertificateModel Certificate, LogModel logModel);
	Task UpdateCertificate(CertificateModel Certificate, LogModel logModel);
	Task DeleteCertificate(int CertificateId, LogModel logModel);
	Task<List<CertificateModel>> Export();
}