# Getting started with Rhino Compute on AWS

1. Prepare your billing account
2. launch instance
3. prepare license files
4. configure instance, inc. installing compute and rhino 7 wip

## 1. Core Hour Billing

1. Go to the [Licenses Portal](https://www.rhino3d.com/licenses?_forceEmpty=true) (Login to your Rhino account if prompted).
2. Click _Create New Team_ and create a team to use for your compute project.
3. Click _Action_ -> _Manage Core-Hour Billing_.
4. Check the checkbox next to Rhino 6 and Rhino 7 and the checkbox signaling you agree to pay.
5. Click _Save_, and enter payment information when prompted for your new team.
6. Once the payment information is saved and core-hour billing is enabled, click _Action_ -> _Get Auth Token_.
7. Copy the token to the clipboard. _WARNING: This token allows anyone with it to charge your team at will. Do NOT share this token with anyone._
8. Set the token as an environment variable named `RHINO_TOKEN`. The token may be too large to set manually in certain versions of Windows. There are easy workarounds:
	- If you want to set the token programmatically as part of a setup script, you can use `Environment.SetEnvironmentVariable` in C#. You can also use PowerShell in a setup script to set `RHINO_TOKEN` programatically.
	- If you want to set `RHINO_TOKEN` manually, launch `RegEdit`, type `Computer\HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Environment` in the address var and press enter. Next, create a new value named `RHINO_TOKEN` with the value copied from the Licenses Portal. You may have to restart Windows for changes to take effect.
   
## 2. Launch instance

Things to pay attention to...

* AMI: "Microsoft Windows Server 2019 Base"
* Recommended instance type: t2.medium
* Set a public ip
* Set a "Name" tag to help keep track of instances
* Configure security group to allow Compute traffic
    * RDP - 3389 TCP
    * HTTP - 80 TCP
    * HTTPS - 443 TCP
* Key pair

## 3. Configure instance

* Remote desktop into the instance
* Copy cloudzoo.json and the .lic file onto the desktop of the remote machine
* Run the command below in a powershell window (change `EMAIL` and `API_KEY` to your email address and an API key of your choice)

<pre>
iwr -useb https://raw.githubusercontent.com/mcneel/compute.rhino3d/master/script/bootstrap-server.ps1 -outfile bootstrap.ps1
.\bootstrap.ps1 -emailaddress <i><b>EMAIL</b></i> -apikey <i><b>API_KEY</b></i> -install
</pre>
