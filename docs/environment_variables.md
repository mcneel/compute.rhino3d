### Environment variables

All configuration of Compute is done via [environment variables](https://helpdeskgeek.com/how-to/create-custom-environment-variables-in-windows/).

| Environment variable | Type | Default | Description |
| -------------------- | ---- | ------- | ----------- |
| `COMPUTE_HTTP_PORT` | `integer` | `80` (Release), `8888` (Debug) | Port to run HTTP server |
| `COMPUTE_HTTPS_PORT` | `integer` | `0` | Port to run HTTPS server |
| `COMPUTE_SPAWN_GEOMETRY_SERVER` | `bool` | `true` (Release), `false` (Debug) | When True, `compute.frontend` will spawn `compute.geometry` at http://localhost on port `COMPUTE_BACKEND_PORT`. Defaults to `false` in Debug so that you can run both `compute.geometry` and `compute.frontend` in the debugger. Configure this in `Solution > Properties > Startup Project`. |
| `COMPUTE_BACKEND_PORT` | `integer` | `8081` | Sets the TCP port where `compute.geometry` runs. |
| `COMPUTE_AUTH_METHOD` | `string` | | `RHINO_ACCOUNT`: Enables authentication via Rhino Accounts OAuth2 Token. Get your token at https://www.rhino3d.com/compute/login and pass it using a Bearer Authentication header in your HTTP request: `Authorization: Bearer <YOUR TOKEN>`.<br>`API_KEY`: Enables athentication via simple API key that looks like an email address. |
| `COMPUTE_LOG_RETAIN_DAYS` | `integer` | `10` | Delete log files after 10 days. |
| `COMPUTE_LOG_CLOUDWATCH` | `bool` | `false` | Stream logs to Amazon CloudWatch. |
| `COMPUTE_STASH_METHOD` | `string` | `TEMPFILE` | `TEMPFILE`: Enables stashing POST input data to a temp file.<br>`AMAZONS3`: Enables stashing POST input data to an Amazon S3 bucket. |
| `COMPUTE_STASH_S3_BUCKET` | `string` | | Name of the Amazon S3 bucket where POST input data should be stashed. Requires `COMPUTE_STASH_METHOD=AMAZONS3` |
| `AWS_ACCESS_KEY` | `string` | | Amazon Web Services Access Key for your account. If compute is running on EC2, consider using [EC2 Instance Profiles](https://docs.aws.amazon.com/IAM/latest/UserGuide/id_roles_use_switch-role-ec2_instance-profiles.html); Compute will find and use your credentials so they don't need to be on your instance. |
| `AWS_SECRET_ACCESS_KEY` | `string` | | Amazon Web Services Secrete Access Key for your account. If compute is running on EC2, consider using [EC2 Instance Profiles](https://docs.aws.amazon.com/IAM/latest/UserGuide/id_roles_use_switch-role-ec2_instance-profiles.html); Compute will find and use your credentials so they don't need to be on your instance. |
| `AWS_REGION_ENDPOINT` | `string` | `"us-east-1"` | Amazon Web Services [Region Endpoint](https://docs.aws.amazon.com/general/latest/gr/rande.html) |
<!-- | `COMPUTE_LOG_METHOD` | `string` | `TEMPFILE` | `TEMPFILE`: Enables logging to the temp directory. | -->

