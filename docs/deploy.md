# Deploying Rhino Compute

This is a short guide to deploying Compute to a server environment. To run Compute locally for development and testing, see [develop.md](develop.md).

1. Set up Core-Hour Billing
2. Prepare Windows Server
3. Install Rhino and Compute
4. Verify Compute and license usage

## 1. Set up Core-Hour Billing

Core-hour billing is required when running Rhino on a Windows Server-based operating system.

1. Go to the [Licenses Portal](https://www.rhino3d.com/licenses?_forceEmpty=true) (Login to your Rhino account if prompted).
2. Click _Create New Team_ and create a team to use for your compute project.
3. Click _Action_ -> _Manage Core-Hour Billing_.
4. Check the checkbox next to Rhino 6 and Rhino 7 and the checkbox signaling you agree to pay.
5. Click _Save_, and enter payment information when prompted for your new team.
6. Once the payment information is saved and core-hour billing is enabled, click _Action_ -> _Get Auth Token_.
7. We'll pass this token to the bootstrap script in the next step to set the `RHINO_TOKEN` environment variable on the virtual machine. Just leave the page open for now.

⚠️ _**WARNING:** This token allows anyone with it to charge your team at will. Do **NOT** share this token with anyone._

## 2. Prepare Windows Server

To run Compute you'll need a server or virtual machine pre-installed with Windows Server 2019.

We'll assume you're deploying Compute to one of Amazon's EC2 instances. There are a few things to pay attention to when setting up the instance – use this as a rough guide if you're using a virtual machine from another cloud provider or a physical server.

* Start with the "Microsoft Windows Server 2019 Base" AMI.
* The t2.medium instance type (2 vCPU, 4 GB RAM) is recommended.
* Assign a public ip, or better yet use an [Elastic IP and Route53](https://docs.aws.amazon.com/Route53/latest/DeveloperGuide/routing-to-ec2-instance.html).
* Set a "Name" tag to help keep track of instances.
* Configure the security group to allow Compute traffic:
    * RDP - 3389 TCP
    * HTTP - 80 TCP
    * HTTPS - 443 TCP

Wait for the virtual machine to spin up... ☕️

## 3. Install Rhino and Compute

On the virtual machine, copy and paste the command below into a powershell window and hit Enter. You will be asked to enter a few things...

* `EmailAddress` - the Rhino download link requires a valid email address
* `ApiKey` - configures an API key to secure the server
* `RhinoToken` – the long token from your core-hour billing team

```powershell
iwr -useb https://raw.githubusercontent.com/mcneel/compute.rhino3d/master/script/bootstrap-server.ps1 -outfile bootstrap.ps1; .\bootstrap.ps1 -install
```
At the end of the installation process, Windows will restart to complete the setup. Wait a minute and log back in to check that the compute.geometry service is running. _You may need to start it manually the first time (only)._

## 4. Verify Compute and license usage

1. Open a browser and go to http://public-dns-or-ip/version. If Compute is working it will return its version and Rhino's version.
1. Visit https://www.rhino3d.com/licenses
1. Under **Team Licenses** click your new team.
1. Verify that Rhino is in use in your core-hour billing team.