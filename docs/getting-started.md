# Getting started with Rhino Compute on AWS

1. launch instance
2. prepare license files
3. configure instance, inc. installing compute and rhino 7 wip

## 1. Launch instance

Things to pay attention to...

* AMI: "Microsoft Windows Server 2019 Base"
* Recommended instance type: t2.medium
* Set a public ip
* Set a "Name" tag to help keep track of instances
* Configure security group to allow Compute traffic
    * HTTP - 80 TCP
    * HTTPS - 443 TCP
* Key pair

## 2. Prepare license files

On your local machine...

* Configure Rhino to use plain text token and log out
* Log in using service account (link to steps)
* Save cloudzoo.json and .lic file somewhere safe

## 3. Configure instance

* Remote desktop into the instance
* Copy cloudzoo.json and the .lic file onto the desktop of the remote machine
* Run the command below in a powershell window (change `EMAIL` and `API_KEY` to your email address and an API key of your choice)

<pre>
iwr -useb https://raw.githubusercontent.com/mcneel/compute.rhino3d/will/bootstrap-complete/script/bootstrap-server.ps1 -outfile bootstrap.ps1
.\bootstrap.ps1 -emailaddress <i><b>EMAIL</b></i> -apikey <i><b>API_KEY</b></i> -install
</pre>