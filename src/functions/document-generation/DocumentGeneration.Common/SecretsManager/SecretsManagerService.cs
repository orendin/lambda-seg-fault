using Amazon.SecretsManager.Extensions.Caching;
using Amazon.SecretsManager;
using Amazon;
using Amazon.Runtime.CredentialManagement;
using Amazon.Runtime;

namespace DocumentGeneration.Common.SecretsManager;

// https://docs.aws.amazon.com/secretsmanager/latest/userguide/retrieving-secrets_cache-net.html
public class SecretsManagerService : ISecretsManagerService
{
  private const string DefaultRegion = "ap-southeast-2";
  private readonly TimeSpan SecretManagerTimeout = TimeSpan.FromSeconds(5);
  private SecretsManagerCache? _cache;

  //separate property to prevent cache from being initialized during construction of the class
  private SecretsManagerCache Cache
  {
    get
    {
      if (_cache == null)
      {
        _cache = new SecretsManagerCache(GetCacheConfiguration());
      }

      return _cache;
    }
  }

  public SecretsManagerService()
  {

  }

  /// <summary>
  /// Returns the cache entry for the specified secret if it exists in the cache. Otherwise,
  /// retrieves the secret from Secrets Manager and creates a new cache entry.
  /// </summary>
  /// <returns> Iron pdf's license key </returns>
  public async Task<string> GetSecretFromSecretsManager(string secretName)
  {
    // manually cancel retrieval using cancellactionToken after configured timeout since
    // asynchronous timeouts don't seem to be honoured by the SecretsManagerClient.
    var tokenSource = new CancellationTokenSource();
    tokenSource.CancelAfter(SecretManagerTimeout);

    return await Cache.GetSecretString(secretName, tokenSource.Token);
  }

  /// <summary>
  /// Get Secret manager's cache configuration for retrieving the secret
  /// </summary>
  public SecretCacheConfiguration GetCacheConfiguration()
  {
    IAmazonSecretsManager client;

    //Multi-stage credential resolution. First try using default profile,
    // if that doesn't work, use the built-in default (aimed at the CI/CD run)
    var chain = new CredentialProfileStoreChain();
    if (chain.TryGetAWSCredentials("default", out var awsCredentials))
    {
      var config = new AmazonSecretsManagerConfig
      {
        Timeout = SecretManagerTimeout,
        RegionEndpoint = RegionEndpoint.GetBySystemName(DefaultRegion)
      };

      client = new AmazonSecretsManagerClient(awsCredentials, config);
    }
    else
    {
      var config = new AmazonSecretsManagerConfig
      {
        Timeout = SecretManagerTimeout
      };

      client = new AmazonSecretsManagerClient(config);
    }

    if (client == null)
    {
      throw new Exception("Could not authenticate with SecretsManager, neither with default profile nor with default EC2 process credentials.");
    }

    return new SecretCacheConfiguration
    {
      CacheItemTTL = 4294967295, // 49.7 days TTL
      VersionStage = "AWSCURRENT",
      Client = client,
    };
  }
}
