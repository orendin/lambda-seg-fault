#!/usr/bin/env node

/**
 * This file is the entry point for CDK into the application. It sources the stacks that CDK will deploy and produces
 * (via app.synth()) the cloudformation templates that CDK will deploy. Only change this file if a new stack is needed
 * to be added to the application.
 */

import { App } from 'aws-cdk-lib';
import { SegFaultStack } from './stacks/seg-fault-api';

const app = new App();
new SegFaultStack(app, 'SegFaultStack', {
  env: {
    account: process.env.CDK_DEFAULT_ACCOUNT,
    region: process.env.CDK_DEFAULT_REGION,
  },
});
