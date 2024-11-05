# Overview / Introduction
This repository provides a solution for paying fees using Swish in the Koha LMS OPAC. The solution is built with .NET Core and integrates with JavaScript within Koha OPAC to facilitate smooth online transactions.

This documentation helps users get started with Swish in a local test environment. For those planning to use this solution in a production environment, please note that an agreement with a Swish-supported bank is required. See the [Production Requirements for Swish Integration](#production-requirements-for-swish-integration) section for more information on how to obtain a Swish number and the necessary certificate for live payments.

# Getting Started
To start using this code, follow these steps to clone the repository, set up the database, and configure the project locally.

## Clone the Repository
Open a terminal and run the following command to clone the repository:
> git clone https://github.com/SodertornHB/online-payment.git 
cd online-payment 

## Create the Database
This project requires a database to store transaction data. Use the SQL script provided to create the necessary database and tables.
1. Open your database management tool (such as SQL Server Management Studio).
1. Locate the SQL create script at ./OnlinePayment.Web/Migration/Migrations.sql.
1. Run the script to set up the database schema.

## Install Dependencies
Ensure that you have .NET Core installed. Run the following command in the project directory to restore all required packages:
> dotnet restore 

## Configuration
To run this application, you need to configure your `appsettings.json` file with the correct settings for your environment. A template file, `appsettings.json.template`, is provided in the repository to guide you through this process.

#### 1. Create a Configuration File
- Copy the template file and rename it to appsettings.json.

- Open appsettings.json and update the fields with your specific settings as follows:

#### 2. Database Connection
- Under ConnectionStrings, replace SERVER_NAMEwith the name of your database server and DATABASE_NAME with the name of your database.

#### 3. Application Settings
**Host**: Specify the host URL where your application will be running.

**Name**: Enter a name for your application.

**KeepLogsInDays**: Define the number of days to retain logs.

**KeysFolder**: Specify the folder path where any necessary keys are stored.

#### 4. IP Blocking Options
**BlockedIPs**: If there are any IP addresses you want to block, add them here as a list.

#### 5. Koha API Settings
**Endpoint**: Enter the endpoint URL for your Koha API.

**UserName and Password**: Enter the credentials for accessing the Koha API.

**LibraryId**: Set the ID of your library within Koha.

**OpacHost**: Specify the OPAC host URL.

#### 6. Swish API Settings
**Endpoint**: Enter the Swish API endpoint URL.

**PayeeAlias**: Enter your Swish Payee Alias (Swish number).

**CallbackUrl**: Specify the URL that Swish should call upon successful payment.

**Currency**: Define the currency code (e.g., SEK for Swedish kronor).

**Message**: Provide a default message to display on Swish payments.

#### 7. Certification Authentication
**Certification**: Specify the file name of the Swish certificate used for authentication.

**Passphrase**: Enter the passphrase for the certificate, if applicable.

**Thumbprint**: If using thumbprint-based authentication, enter the certificate thumbprint here.


Once configured, the application will use these settings to connect with the necessary services and authenticate as required.

# Swish Test Environment
To test payments, you can use Swish's test environment, which simulates the live payment flow. Payments against the test environments are automatically set to status PAID. 

## Base URL
The base URL for the Swish test environment is: https://mss.cpc.getswish.net

## Test Certificate
To connect to the Swish test environment, you need a test certificate. You can download the test certificate from Swish's developer portal at: https://developer.swish.nu/documentation/environments
For testing purposes, the file Swish_Merchant_TestCertificate_1234679304.p12 has been confirmed to work.

## Installing the Certificate
1. Download the test certificate file.
2. Install the certificate on your machine by following these steps:
- Choose **Local Machine** as the installation location.
- Select the **Personal** store.
3. When prompted for a password, use swish.

By configuring this test certificate, you can simulate transactions with the Swish test environment without affecting live data.

# Running the Application
After configuration, you can start the application using the following command:
> dotnet run 

# Integrate with Koha
To integrate the Swish payment functionality with Koha OPAC, you need to add a JavaScript snippet to Koha’s system preferences.

## Adding JavaScript in Koha
1. Go to Koha's system preferences and locate OPACUserJS.
2. Add the following JavaScript code to OPACUserJS:
```javascript
var paymentScript = document.createElement('script');
paymentScript.type = 'text/javascript';
**paymentScript.src = 'https**://YOUR_HOST_NAME/js?borrowernumber=' + $('.loggedinusername').attr('data-borrowernumber') + '&lang=' + $('html').attr('lang');
$('head').append(paymentScript);
```

## Customizing the Host Name
Replace YOUR_HOST_NAME with the host URL where your payment application is hosted.

## How It Works
This script fetches information on whether the logged-in borrower has any fees to pay. If fees are detected, a Swish icon appears on the fees page in Opac. This icon acts as a link, allowing the user to proceed with the payment via Swish.

# Organizational-Specific Files and Configuration
To maintain organization-specific files and configurations separate from the main codebase, this project supports an organizational-specific folder in the root of the solution. Files in this folder will be copied to the corresponding folders in the web project during the build process, allowing for custom settings without modifying the core code. This approach ensures that organization-specific files and configurations are not overwritten when pulling new code from GitHub. It also keeps your customizations separate, making updates and maintenance easier.

## Setting Up the Organizational-Specific Folder
1. In the root of the solution, create a folder named `organizational-specific`.
1. Inside this folder, add any files or configurations specific to your organization. The folder structure should mirror the structure of the web project. The following file types are automatically copied: `.css`, `.js`, `.cs`, `.json`, `.csproj`, `.resx`

### Important Note
Any files in the web project that have the same name and path as files in the organizational-specific folder will be overwritten by the files from organizational-specific during the build process. Be cautious when adding files to avoid unintentional overwrites.

#### Example: Custom appsettings.development.json
If you want to use a custom `appsettings.development.json` file, place it directly in the organizational-specific folder. During the build process, it will be copied to the appropriate location in the web project.

#### Example: Custom Translation Files
To add custom translation files, create a `Resources` folder inside organizational-specific and add files such as `SharedResource.sv.resx` or `SharedResource.en.resx` for different languages.

## Authentication Configuration (Recommended)
It is recommended to configure some form of authentication to secure the application, especially when handling payment-related functionalities. Depending on your organization’s security requirements, consider implementing authentication methods such as OAuth, API key authentication, or certificate-based authentication. Custom authentication settings can be placed in the organizational-specific folder to avoid exposing sensitive information in the main codebase.

## Configuration in the Project File
The copying of files is configured in the .csproj file. Below is the configuration that enables this copying process:
```xml
  <Target Name="CopyOrgSpecificFiles" BeforeTargets="Build">
    <ItemGroup>
      <OrgSpecificFiles Include="..\organizational-specific\**\*.css" />
      <OrgSpecificFiles Include="..\organizational-specific\**\*.js" />
      <OrgSpecificFiles Include="..\organizational-specific\**\*.cs" />
      <OrgSpecificFiles Include="..\organizational-specific\*.json" />
      <OrgSpecificFiles Include="..\organizational-specific\*.csproj" />
    </ItemGroup>

    <Message Text="Copying @(OrgSpecificFiles) to $(ProjectDir)%(OrgSpecificFiles.RecursiveDir)%(Filename)%(Extension)" Importance="high" />

    <Copy SourceFiles="@(OrgSpecificFiles)" DestinationFiles="@(OrgSpecificFiles->'$(ProjectDir)%(RecursiveDir)%(Filename)%(Extension)')" OverwriteReadOnlyFiles="true" />
  </Target>
```
# Production Requirements for Swish Integration
To use this solution in a production environment, you must have an agreement with a Swish-supported bank. Only then will you receive a Swish number, enabling you to log in to the Swish portal and generate the necessary certificate for live payments.

## Steps to Obtain Production Access
1. Contact your bank to set up an agreement for Swish payments in a business context.
2. Once your agreement is in place, you will receive a Swish number.
3. Log in to the Swish portal at https://portal.swish.nu/company.
4. Use the portal to create the certificate required for authenticating live Swish transactions.

This certificate is essential for enabling secure, real-time payments through Swish in production.

# Contributing to the Project
We welcome contributions to this open-source project! By contributing, you help improve the functionality, usability, and reliability of this solution for other users. Follow these steps to get started with your contribution.

## Contribution Guidelines
#### 1. Fork the Repository

Start by forking the repository to your own GitHub account. This creates a copy of the project where you can make your changes.
#### 2. Clone Your Forked Repository

Clone your forked repository to your local machine using the command:
```bash
git clone https://github.com/YOUR_USERNAME/online-payment.git  
```
Replace YOUR_USERNAME with your GitHub username.
#### 3. Create a New Branch

To keep your changes organized, create a new branch for each feature or bug fix you want to work on:
```bash
git checkout -b feature/your-feature-name  
```
Use a descriptive name for your branch, like feature/add-payment-option or bugfix/fix-currency-bug.
#### 4. Make Your Changes

Implement your changes in the codebase. Ensure your code follows the project's coding standards and is properly documented.
#### 5. Test Your Changes

Before submitting your contribution, test your changes thoroughly. If applicable, add or update unit tests to maintain code quality.
#### 6. Commit and Push Your Changes

Stage and commit your changes with a descriptive commit message:
```bash
git add .  
git commit -m Add description of your changes  
```
Push your changes to your forked repository:

```bash
git push origin feature/your-feature-name  
```
#### 7. Submit a Pull Request

Go to the original repository on GitHub and submit a pull request from your forked repository. Include a clear description of your changes, why they are necessary, and any relevant context.
#### 8. Respond to Feedback

Project maintainers may review your pull request and provide feedback or request changes. Please be prepared to make revisions if necessary.