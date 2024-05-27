using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using BSPOS.Core.Contract.Infrastructure;
using BSPOS.Core.Model;

namespace BSPOS.Infrastructure;

public static class ServiceRegistration
{
	public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
	{
		services.Configure<EmailSettingsSendGridModel>(configuration.GetSection("EmailSettings_SendGrid"));
		services.AddScoped<IEmailSender, EmailSenderSendGrid>();

		services.Configure<SMSSettingsModel>(configuration.GetSection("SMSSettings"));
		services.AddScoped<ISMSSender, SMSSenderAlpha>();

		return services;
	}
}