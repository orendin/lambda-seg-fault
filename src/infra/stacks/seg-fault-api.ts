import { Stack, StackProps } from 'aws-cdk-lib';
import { Construct } from 'constructs';
import { SegFaultApiFunctions } from '../constructs/functions';
import { SegFaultRestApi } from '../constructs/rest-api/rest-api';

export class SegFaultStack extends Stack {

  constructor(scope: Construct, id: string, props?: StackProps) {
    super(scope, id, props);

    // Create functions
    const functions = new SegFaultApiFunctions(this, 'SegFaultFunctions');

    // Create rest API
    new SegFaultRestApi(this, 'segFaultApi', functions.documentGenerator);
  }
}
