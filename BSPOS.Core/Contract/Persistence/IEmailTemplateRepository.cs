using BSPOS.Core.Model;

namespace BSPOS.Core.Contract.Persistence;

public interface IEmailTemplateRepository
{
	Task<PaginatedListModel<EmailTemplateModel>> GetEmailTemplates(int pageNumber);
	Task<EmailTemplateModel> GetEmailTemplateById(int emailTemplateId);
	Task<EmailTemplateModel> GetEmailTemplateByName(string name);
	Task UpdateEmailTemplate(EmailTemplateModel emailTemplate, LogModel logModel);
}