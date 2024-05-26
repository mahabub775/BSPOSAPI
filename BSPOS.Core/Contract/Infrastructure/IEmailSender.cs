using SMS.Core.Model;

namespace SMS.Core.Contract.Infrastructure;

public interface IEmailSender
{
	Task SendEmail(EmailModel email);
}