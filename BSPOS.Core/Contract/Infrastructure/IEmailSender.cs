using BSPOS.Core.Model;

namespace BSPOS.Core.Contract.Infrastructure;

public interface IEmailSender
{
	Task SendEmail(EmailModel email);
}