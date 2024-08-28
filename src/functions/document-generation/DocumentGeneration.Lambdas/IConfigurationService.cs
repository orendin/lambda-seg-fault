using Microsoft.Extensions.Configuration;

namespace DocumentGeneration.Lambdas;

public interface IConfigurationService
{
    IConfiguration GetConfiguration();
}
