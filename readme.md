# QuickStart
* There needs to be at least one git commit for GitVersion to be able to generate a gitversion
* Run ./build.ps1 to initiate build & deploy process
* Run ./build.ps1 --target ParameterUpload -> this will upload parameters to parameter store.
* Run ./utils/SecureConfiguration.ps1 script to protect secure config.

# Environment Variables
export AWS_PROFILE=default
export AWS_DEFAULT_REGION=eu-west-2
export Application__LambdaLogSetupStackUrl=https://s3.amazonaws.com/px-automation-cloudformation-stacks/templates/lambdaLogConfig/latest/lambdaLogConfig.yaml
export Application__S3BucketTemplateUrl=https://s3.amazonaws.com/px-automation-cloudformation-stacks/templates/s3/latest/s3.yaml
export Application__KMSKey=arn:aws:kms:eu-west-2:637422166946:key/82e9836b-99bf-4758-b30c-70819abe4823
export Application__KmsKeyArn=arn:aws:kms:eu-west-2:637422166946:key/82e9836b-99bf-4758-b30c-70819abe4823
export Application__LambdaRoleTemplateUrl=https://s3.amazonaws.com/px-automation-cloudformation-stacks/templates/lambdaRole/latest/lambdaRole.yaml
export Application__QueueTemplateUrl=https://s3.amazonaws.com/px-automation-cloudformation-stacks/templates/queue/latest/queue.yaml
export Application__ApiGatewayDomainName=VehicleNotificationService.dev.parknowportal.com
export Application__S3AccessLogsToBucket=false
export Application__AppEnvironment=dev
export Application__Environment=dev
