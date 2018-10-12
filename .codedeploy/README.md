# CodeDeploy

To deploy...

1. create a `src/bin/Release/` directory in here and fill it with the
   contents of `../src/bin/Release/`
2. Run `aws deploy push --application-name <app_name> --s3-location <s3://bucket/key.zip>`
   to create a new deployment revision
3. Deploy the new revision by following the CLI instructions or via the AWS
   console
