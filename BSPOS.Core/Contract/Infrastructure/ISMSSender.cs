using SMS.Core.Model;

namespace SMS.Core.Contract.Infrastructure;

public interface ISMSSender
{
	Task SendSMS(SMSModel sms);
}