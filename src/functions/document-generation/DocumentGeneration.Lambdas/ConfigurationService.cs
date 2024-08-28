using Microsoft.Extensions.Configuration;

namespace DocumentGeneration.Lambdas;

public class ConfigurationService : IConfigurationService
{
  public IConfiguration GetConfiguration()
  {
    return new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: true)
        .AddEnvironmentVariables()
        .Build();
  }
}
