# Using Compute in Production

To use Compute in production requires several major elements:

1. [Set up a core-hour billing team in Rhino Accounts.](#1-core-hour-billing)
1. Launch a virtual machine
1. Install Rhino and Compute
1. Start the compute service on your virtual machine
1. Verifying license usage

## 1. Core Hour Billing

1. Go to the [Licenses Portal](https://www.rhino3d.com/licenses?_forceEmpty=true) (Login to your Rhino account if prompted).
2. Click _Create New Team_ and create a team to use for your compute project.
3. Click _Action_ -> _Manage Core-Hour Billing_.
4. Check the checkbox next to Rhino 7 and the checkbox signaling you agree to pay.
5. Click _Save_, and enter payment information when prompted for your new team.
6. Once the payment information is saved and core-hour billing is enabled, click _Action_ -> _Get Auth Token_.
7. Copy the token to the clipboard. _WARNING: This token allows anyone with it to charge your team at will. Do NOT share this token with anyone._
8. Set the token as an environment variable named `RHINO_TOKEN`. The token may be too large to set manually in certain versions of Windows. There are easy workarounds:
	- If you want to set the token programmatically as part of a setup script, you can use `Environment.SetEnvironmentVariable` in C#. You can also use PowerShell in a setup script to set `RHINO_TOKEN` programatically.
	- If you want to set `RHINO_TOKEN` manually, launch `RegEdit`, type `Computer\HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Environment` in the address var and press enter. Next, create a new value named `RHINO_TOKEN` with the value copied from the Licenses Portal. You may have to restart Windows for changes to take effect.

## 3. Launch Your Virtual Machine

1. **TODO:** Steve's instructions

## 4. Install Rhino and Compute

1. **TODO:** Powershell script

## 5. Start the Compute Service

1. **TODO:** instructions

## 6. Verify License Usage

1. Visit https://www.rhino3d.com/licenses
1. Under **Team Licenses** click your new team
1. Verify that Rhino is in use in your Core-Hour billing team.
1. Stop the Compute service.
1. Verify that Rhino is no longer in use in your Core-Hour billing team.
