using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using DocumentGeneration.Common.SecretsManager;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Reflection;
using AWS.Lambda.Powertools.Logging;
using Microsoft.Extensions.Logging;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace DocumentGeneration.Lambdas;

public class GeneratorFunction
{
  private readonly PdfGenerator _generator;
  private readonly ILogger _logger;

  static GeneratorFunction()
  {
    var logger = Logger.Create<GeneratorFunction>();
    logger.LogDebug("Static Generator Function triggered");
    PdfGenerator.InitializeIronPdf(logger);

    AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionHandler;

    logger.LogDebug("Static Generator Function finished.");
  }

  /// <summary>
  /// Parameterless constructor required for Lambda to start
  /// </summary>
  public GeneratorFunction()
  {
    var serviceProvider = BuildServiceProvider();

    _logger = serviceProvider.GetService<ILogger>() ?? throw new Exception("Logger could not be resolved.");
    _generator = serviceProvider.GetService<PdfGenerator>() ?? throw new Exception("PdfGenerator could not be resolved.");
    _logger.LogDebug("GeneratorFunction parameterless constructor finished.");
  }

  public GeneratorFunction(IServiceProvider? serviceProvider = null)
  {
    if (serviceProvider == null)
    {
      serviceProvider = BuildServiceProvider();
    }

    _generator = serviceProvider.GetService<PdfGenerator>() ?? throw new Exception("PdfGenerator could not be resolved.");
    _logger = serviceProvider.GetService<ILogger>() ?? throw new Exception("Logger could not be resolved.");

    _logger.LogDebug("GeneratorFunction parameterized constructor finished.");
  }

  [Logging]
  public APIGatewayProxyResponse DocumentGeneratorHandler(APIGatewayProxyRequest request, ILambdaContext context)
  {
    _logger.LogDebug("DocumentGeneratorHandler triggered.");

    try
    {
      var basePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "", "Templates");
      var pdfData = _generator.Generate(basePath);

      _logger.LogInformation("Document generation finished.");

      return new APIGatewayProxyResponse
      {
        StatusCode = (int)HttpStatusCode.Created,
        Body = pdfData,
        Headers = new Dictionary<string, string> { { "Content-Type", "application/pdf" } },
        IsBase64Encoded = true
      };

    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Document generation failed");

      return new APIGatewayProxyResponse
      {
        StatusCode = (int)HttpStatusCode.BadRequest,
        Body = $"Error during document generation: {ex?.Message}",
        Headers = new Dictionary<string, string> { { "Content-Type", "text/plain" } },
        IsBase64Encoded = false
      };
    }
    finally
    {
      //enable this to fix the issue.
      //GC.Collect();
    }
  }
  private static ServiceProvider BuildServiceProvider()
  {
    var serviceCollection = new ServiceCollection();

    serviceCollection.AddTransient<DocumentGenerationSettings>();
    serviceCollection.AddTransient<IConfigurationService, ConfigurationService>();
    serviceCollection.AddTransient<ISecretsManagerService, SecretsManagerService>();
    serviceCollection.AddTransient<PdfGenerator>();
    serviceCollection.AddSingleton(serviceProvider => Logger.Create<GeneratorFunction>());

    return serviceCollection.BuildServiceProvider();
  }

  static void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs e)
  {
    try
    {
      var logger = Logger.Create<GeneratorFunction>();
      logger.LogError("Unhandled Exception: {Exception}", e.ExceptionObject);
    }
    catch
    {
      //to nothing since we can't log
    }
  }
}

