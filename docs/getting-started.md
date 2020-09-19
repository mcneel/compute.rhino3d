# Getting started with Rhino Compute

This is a short guide to deploying Compute to a Windows Server computer or virtual machine. This is not the guide for setting up Compute locally for development and testing.

1. Prepare Windows
2. Set up Core-Hour Billing
3. Install Rhino and Compute

## 1. Prepare Windows

To run Compute you'll need a server or virtual machine pre-installed with Windows Server 2019.

As an example, you can deploy Compute to one of Amazon's EC2 instances. Here are a few things to pay attention to when setting up the virtual machine...

* AMI: "Microsoft Windows Server 2019 Base"
* Recommended instance type: t2.medium (2 vCPU, 4 GB RAM)
* Set a public ip, or better yet use an [Elastic IP and Route53](https://docs.aws.amazon.com/Route53/latest/DeveloperGuide/routing-to-ec2-instance.html)
* Set a "Name" tag to help keep track of instances
* Configure the security group to allow Compute traffic
    * RDP - 3389 TCP
    * HTTP - 80 TCP
    * HTTPS - 443 TCP

While you're waiting for the virtual machine to spin up, move on to step 2.

## 2. Set up Core-Hour Billing

1. Go to the [Licenses Portal](https://www.rhino3d.com/licenses?_forceEmpty=true) (Login to your Rhino account if prompted).
2. Click _Create New Team_ and create a team to use for your compute project.
3. Click _Action_ -> _Manage Core-Hour Billing_.
4. Check the checkbox next to Rhino 6 and Rhino 7 and the checkbox signaling you agree to pay.
5. Click _Save_, and enter payment information when prompted for your new team.
6. Once the payment information is saved and core-hour billing is enabled, click _Action_ -> _Get Auth Token_.
<!-- TODO -->
7. Copy the token to the clipboard.
7. We'll use this token to set the `RHINO_TOKEN` environment variable on the virtual machine.

⚠️ _**WARNING:** This token allows anyone with it to charge your team at will. Do **NOT** share this token with anyone._

## 3. Install Rhino and Compute

On the virtual machine, open a powershell window and run the command below. Change `EMAIL` and `API_KEY` to your email address and an API key of your choice. At the end of the installation process, Windows will restart.

<pre>
iwr -useb https://raw.githubusercontent.com/mcneel/compute.rhino3d/master/script/bootstrap-server.ps1 -outfile bootstrap.ps1
.\bootstrap.ps1 -emailaddress <i><b>EMAIL</b></i> -apikey <i><b>API_KEY</b></i> -install
</pre>

```powershell
iwr -useb https://raw.githubusercontent.com/mcneel/compute.rhino3d/master/script/bootstrap-server.ps1 -outfile bootstrap.ps1; .\bootstrap.ps1 -install
```

<!-- TODO: test bootstrap script on windows and add RHINO_TOKEN -->

The script will ask you for your email address, 

_Arguments_

* `-EmailAddress EMAIL` - the Rhino download link requires a valid email
* `-ApiKey KEY` - set an API key to secure the server
* `-install` - (optional) install the compute.geometry service

