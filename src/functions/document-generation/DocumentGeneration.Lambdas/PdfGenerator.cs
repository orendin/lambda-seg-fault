using Microsoft.Extensions.Logging;

namespace DocumentGeneration.Lambdas;

public class PdfGenerator
{
  private readonly ILogger _logger;
  private readonly ChromePdfRenderer _renderer;

  public PdfGenerator(DocumentGenerationSettings documentGenerationSettings, ILogger logger)
  {
    _logger = logger;

    _renderer = new ChromePdfRenderer
    {
      RenderingOptions =
      {
        UseMarginsOnHeaderAndFooter = UseMargins.None,
        CreatePdfFormsFromHtml = true
      }
    };

    if (string.IsNullOrEmpty(documentGenerationSettings.IronPdfKey))
    {
      _logger.LogError("PdfGenerator contructor: missing IronPdf key.");

      throw new Exception("IronPdfKey was not provided but is required for the generator to run.");
    }

    License.LicenseKey = documentGenerationSettings.IronPdfKey;
  }

  public static void InitializeIronPdf(ILogger logger)
  {
    var logLevel = Environment.GetEnvironmentVariable("POWERTOOLS_LOG_LEVEL");
    //add additional Iron Pdf logging when debug level is set
    if (logLevel == "Debug")
    {
      IronPdf.Logging.Logger.LoggingMode = IronPdf.Logging.Logger.LoggingModes.Custom;
      IronPdf.Logging.Logger.CustomLogger = logger;
    }

    // we configure the dependencies ourselves in the Dockerfile
    Installation.LinuxAndDockerDependenciesAutoConfig = false;
    // don't do this because it will cause lambda to timeout in the init phase
    Installation.AutomaticallyDownloadNativeBinaries = false;
    Installation.SendAnonymousAnalyticsAndCrashData = false;

    var awsTmpPath = "/tmp/";
    Environment.SetEnvironmentVariable("TEMP", awsTmpPath, EnvironmentVariableTarget.Process);
    Environment.SetEnvironmentVariable("TMP", awsTmpPath, EnvironmentVariableTarget.Process);
    Installation.TempFolderPath = awsTmpPath;
    Installation.CustomDeploymentDirectory = awsTmpPath;

    Installation.ChromeGpuMode = IronPdf.Engines.Chrome.ChromeGpuModes.Disabled;
    Installation.ChromeBrowserCachePath = awsTmpPath;
    Installation.CustomDeploymentDirectory = awsTmpPath;

    Installation.Initialize();
  }

  public string Generate(string templateRootPath)
  {
    using PdfDocument pdf = RenderPdf(templateRootPath);

    return Convert.ToBase64String(pdf.Stream.ToArray());
  }

  public PdfDocument RenderPdf(string templateRootPath)
  {
    var indexFilePath = Path.Combine(templateRootPath, "index.html");
    var rawHtmlTemplate = File.ReadAllText(indexFilePath);

    _logger.LogDebug("Starting IronPdf RenderHtmlAsPdf...");

    var document = _renderer.RenderHtmlAsPdf(rawHtmlTemplate, templateRootPath);

    return document;
  }
}
