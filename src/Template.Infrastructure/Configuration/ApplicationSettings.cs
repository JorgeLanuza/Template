using BaseCore.Framework.Configuration.Certificates;
using BaseCore.Framework.Configuration.ConnectionString;
using BaseCore.Framework.Configuration.Interfaces;

namespace Template.Infrastructure.Configuration;

public class ApplicationSettings : IBaseCoreApplicationSettings
{
	public string? ApplicationName { get; set; } = "TemplateApp";

	public CertificateConfiguration CertificateSettings { get; set; } = new();

	public List<BaseCoreConnectionString> ConnectionStrings { get; set; } = new();

	public string LoggingSettings { get; set; } = "Information";

	public string IdentityServerUrl { get; set; } = "";

	public List<OwnEncodedString> OwnEncodedStrings { get; set; } = new();

	public Dictionary<string, object> OwnCustomSettings { get; set; } = new();

	public List<OwnEncodedString> OwnCertificates { get; set; } = new();

	public string? TrackLoggerSettings { get; set; }

	public bool DiagnosticEnabled { get; set; }

	public bool ResetTokenTimeout { get; set; } = true;
}
