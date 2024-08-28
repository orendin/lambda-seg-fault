using Amazon.SecretsManager.Extensions.Caching;

namespace DocumentGeneration.Common.SecretsManager;

public interface ISecretsManagerService
{
  public Task<string> GetSecretFromSecretsManager(string secretName);
  public SecretCacheConfiguration GetCacheConfiguration();
}
