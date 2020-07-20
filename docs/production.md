# Using Compute in Production

To use Compute in production requires several major elements:

1. [Set up a core-hour billing team in Rhino Accounts.](#1-core-hour-billing)
1. Obtain an OAuth2 token to be used on every compute instance.
1. Launch a virtual machine
1. Install Rhino and Compute
1. Configure your virtual machine with your OAuth2 token
1. Start the compute service on your virtual machine
1. Verifying license usage

## 1. Core Hour Billing

1. Select an `admin email address` that will be used to administer your core-hour billing team. This address will receive bills and core-hour billing-related notifications.
1. Visit https://accounts.rhino3d.com, and login using your `admin email address`.
1. Click **Teams**
1. Click **New Team**
1. Enter the name and description for your new team, then click **Create New Team**
1. From the **Action** menu, click **Edit Payment Information...**, then click **Add Payment Information**
1. Enter your credit card details, then click **Save**
1. Visit https://www.rhino3d.com/licenses
1. Under **Team Licenses** click your new team
1. Click the **Manage Team** button, then click **Manage Core-Hour Billing**
1. Select the checkboxes next to `Rhino 6` and `Rhino 7`.
1. Read the terms and conditions, select the checkbox next to "I agree", and click **Save**

## 2. Obtain OAuth2 Token for Compute Instances

### 2.a. Create a Service Account

1. Open a private browsing window in your web browser.
1. Choose a `service email address` that will be used *only* on your compute instances.
1. Ensure that your `service email address` does not have a Rhino account, and that you can receive emails for the account.
1. Visit https://accounts.rhino3d.com/ in the private browsing window and create an account for your `service email address`
1. Leave your private browsing window open; you'll need it again soon.

### 2.b. Invite the Service Account to your Core-Hour Billing Team

1. Open your normal web browsing window.
1. Visit https://accounts.rhino3d.com/
1. Click **Teams**, then click your **Core-Hour Billing Team**
1. Click **Actions**, then click **Invite Members**
1. Invite `service email address` to the team

### 2.c. Accept the invitation

1. Switch to your private browsing window.
1. Open the email for your `service email address`
1. Click the link from Rhino Accounts to accept the invitation to the team.

### 2.d. Obtain an OAuth2 Token for your service account

Rhino encrypts license information by default. In order to create an image and scale your compute service, you'll need to disable encryption of the license information before creating your machine image. This is also a requirement for creating a [Docker image](../Dockerfile).

*Warning: do not share your cloudzoo.json file - it allows people to use Rhino and bill your core-hour team.*

1. Start Rhino on your development computer.
1. From the **Tools** menu, click **Options** then click **Advanced**
1. Search for `Rhino.LicensingSettings.CloudZooPlainText`
1. Set `CloudZooPlainText` to True.
1. Search for `Rhino.LicensingSettings.ManualEntitySelection`
1. Set `ManualEntitySelection` to True. 
1. Click OK to close the Options dialog box.
1. Run the `Logout` command to logout of Rhino. 
1. ⚠️ **Important!** Close all instances of Rhino – changes do not take effect until Rhino is restarted
1. Start Rhino
1. Login to your Service Account
1. Select your Core Hour Billing Team, then click Get License
1. Close Rhino
1. Create a "Compute Licenses" folder on your desktop
1. Copy `%appdata%\McNeel\Rhinoceros\6.0\License Manager\Licenses\cloudzoo.json` to your Compute Licenses folder.
1. Copy `%programdata%\McNeel\Rhinoceros\6.0\License Manager\Licenses\{GUID}.lic`  to your Compute Licenses folder. Note that the GUID that names this file changes with each version of Rhino. Rhino 6 and Rhino 7 WIP use 55500d41-3a41-4474-99b3-684032a4f4df.

## 3. Launch Your Virtual Machine

1. **TODO:** Steve's instructions

## 4. Install Rhino and Compute

1. **TODO:** Powershell script

## 5. Configure Your VM to use your Core-Hour Billing token

1. Copy the `cloudzoo.json` file from step 2 to `%appdata%\McNeel\Rhinoceros\6.0\License Manager\Licenses\cloudzoo.json` on your VM.
1. Copy the `{GUID}.lic` file from step 2 to `%programdata%\McNeel\Rhinoceros\7.0\License Manager\Licenses\{GUID}.lic` on your VM.
1. Copy the `Settings-Scheme__Default.xml` file from step 2 to `%appdata%\McNeel\Rhinoceros\7.0\settings\Settings-Scheme__Default.xml` on your VM.

## 6. Start the Compute Service

1. **TODO:** instructions

## 7. Verify License Usage

1. Visit https://www.rhino3d.com/licenses
1. Under **Team Licenses** click your new team
1. Verify that Rhino is in use in your Core-Hour billing team.
1. Stop the Compute service.
1. Verify that Rhino is no longer in use in your Core-Hour billing team.
