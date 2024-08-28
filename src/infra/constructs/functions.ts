import { Duration } from 'aws-cdk-lib';
import { DockerImageCode, DockerImageFunction } from 'aws-cdk-lib/aws-lambda';
import { Secret } from 'aws-cdk-lib/aws-secretsmanager';
import { Construct } from 'constructs';

export class SegFaultApiFunctions extends Construct {
  public readonly documentGenerator: DockerImageFunction;

  constructor(scope: Construct, id: string) {
    super(scope, id);

    // Retrieve secrets
    const ironPdfSecret = new Secret(this, 'SegFault.IronPdf.License.LicenseKey', {
      secretName: 'SegFault.IronPdf.License.LicenseKey',
      description: 'Used by the document generation service for generating PDFs using the IronPdf library.',
    });

    // set-up document generator lambda as a container image
    this.documentGenerator = new DockerImageFunction(this, 'DocumentGenerator', {
      code: DockerImageCode.fromImageAsset('./src/functions/document-generation'),
      memorySize: 2048,
      timeout: Duration.seconds(120),
      functionName: `document-generator`,
      description: 'Generates PDF documents',
    });
    this.documentGenerator.addEnvironment('POWERTOOLS_SERVICE_NAME', 'document-generation');
    this.documentGenerator.addEnvironment('POWERTOOLS_LOG_LEVEL', 'Information');

    // grant read permissions on the IronPdf secret to the document generator lambda.
    ironPdfSecret.grantRead(this.documentGenerator);
  }
}
