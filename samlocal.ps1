sam local start-api --debug --template template.yaml --env-vars samlocal_envvar.json --profile playpenprofile --parameter-overrides "ParameterKey=Environment,ParameterValue=dev ParameterKey=Platform,ParameterValue=parknow ParameterKey=System,ParameterValue=phonixx ParameterKey=SubSystem,ParameterValue=vehicle-notification ParameterKey=AppEnvironment.ParameterValue=dev ParameterKey=Version,ParameterValue=0.1.0 ParameterKey=lambdaRoleTemplateUrl,ParameterValue=https://s3.amazonaws.com/px-automation-cloudformation-stacks/templates/lambdaRole/latest/lambdaRole.yaml ParameterKey=queueTemplateUrl,ParameterValue=https://s3.amazonaws.com/px-automation-cloudformation-stacks/templates/queue/latest/queue.yaml ParameterKey=lambdaLogSetupStackUrl,ParameterValue=https://s3.amazonaws.com/px-automation-cloudformation-stacks/templates/lambdaLogConfig/latest/lambdaLogConfig.yaml ParameterKey=S3BucketTemplateUrl,ParameterValue=https://s3.amazonaws.com/px-automation-cloudformation-stacks/templates/lambdaLogConfig/latest/s3.yaml"