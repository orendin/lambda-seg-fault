import { RemovalPolicy } from 'aws-cdk-lib';
import { AccessLogFormat, EndpointType, LambdaIntegration, LogGroupLogDestination, MethodLoggingLevel, RestApi, RestApiProps} from 'aws-cdk-lib/aws-apigateway';
import { HttpMethod } from 'aws-cdk-lib/aws-events';
import { IFunction } from 'aws-cdk-lib/aws-lambda';
import { LogGroup, RetentionDays } from 'aws-cdk-lib/aws-logs';
import { Construct } from 'constructs';

export class SegFaultRestApi extends Construct {
  constructor(scope: Construct, id: string, docGenFunction: IFunction) {
    super(scope, id);

    const apiConfiguration: RestApiProps = {
      description: 'Doc Gen Api - Fault reproduction endpoint',
      endpointTypes: [EndpointType.REGIONAL],
      restApiName: `Doc Gen Api`,
      deployOptions: {
        stageName: 'v1',
        loggingLevel: MethodLoggingLevel.ERROR,
        accessLogDestination: new LogGroupLogDestination(
          new LogGroup(this, 'APIGatewayLogGroup', {
            logGroupName: `/aws/apigateway/seg-fault-api-gateway-access-logs`,
            retention: RetentionDays.ONE_MONTH,
            removalPolicy: RemovalPolicy.DESTROY, //destroy the log group if the stack is destroyed
          }),
        ),
        accessLogFormat: AccessLogFormat.jsonWithStandardFields(),
      },
      binaryMediaTypes: ['*/*'],
    };

    const restApi = new RestApi(this, 'SegFaultApi', apiConfiguration);

    // document-generation
    const documentGeneration = restApi.root.addResource('document-generation');
    documentGeneration.addMethod(HttpMethod.POST, new LambdaIntegration(docGenFunction));
  }
}
