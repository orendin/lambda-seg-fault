using DocumentGeneration.Common.SecretsManager;
using Microsoft.Extensions.Logging;
using System.Configuration;

namespace DocumentGeneration.Lambdas;

public class DocumentGenerationSettings
{
  public const string IronPdfSecretName = "SegFault.IronPdf.License.LicenseKey";

  private readonly IConfigurationService _configurationService;
  private readonly ILogger _logger;

  public string IronPdfKey { get; }

  public DocumentGenerationSettings(IConfigurationService configurationService, ISecretsManagerService secretsManagerService, ILogger logger)
  {
    _configurationService = configurationService;
    _logger = logger;

    // Two-step resolution of the IronPdf key to enable easier local development.
    // First we try getting the key from a appsettings.local.json file. If that doesn't exist,
    // get the key from SecretManager in AWS.
    try
    {

      var configuration = _configurationService.GetConfiguration();
      var potentialLicenseKey = configuration[IronPdfSecretName];

      if (string.IsNullOrEmpty(potentialLicenseKey))
      {
        _logger.LogDebug("IronPdf key not found in local configuration. Trying resolution via SecretsManager.");

        potentialLicenseKey = secretsManagerService.GetSecretFromSecretsManager(IronPdfSecretName).Result;
      }

      if (string.IsNullOrEmpty(potentialLicenseKey))
      {
        throw new ConfigurationErrorsException("IronPdf key is missing. Either configure the value in secrets manager or in local appsettings for development.");
      }

      IronPdfKey = potentialLicenseKey;

      _logger.LogDebug("IronPdf LicenseKey resolution successful.");
    }
    catch (Exception ex)
    {
      _logger.LogError("Error during Settings resolution for Document Generation. Message: {ExceptionMessage}", ex?.Message);

      throw;
    }
  }

}
