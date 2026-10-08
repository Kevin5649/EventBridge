============================================================
EVENTBRIDGE – EVENT PLANNER MARKETPLACE
PROJECT SETUP & RUN GUIDE
============================================================

This guide is for anyone who clones the EventBridge repository and
needs to configure and run the project locally.


============================================================
1. PROJECT OVERVIEW
============================================================

EventBridge is a web-based marketplace connecting customers with
trusted event planners.

CUSTOMER WORKFLOW
-----------------
Registration/Login -> Search Planner -> View Profile -> Send Enquiry
-> Receive Quotation -> Book Event -> Make Payment -> Complete Event
-> Submit Review

EVENT PLANNER WORKFLOW
----------------------
Registration/Login -> Manage Profile & Portfolio -> Receive Enquiry
-> Send Quotation -> Manage Booking -> Update Booking Status

ADMIN WORKFLOW
--------------
Login -> Manage Users -> Verify Planners -> Manage Platform


============================================================
2. TECHNOLOGY STACK
============================================================

- ASP.NET Core MVC (.NET 8)
- C#
- Razor Views
- HTML5
- CSS3
- JavaScript
- Bootstrap
- Entity Framework Core
- Microsoft SQL Server
- ASP.NET Core Identity
- Razorpay API
- Visual Studio 2022
- Visual Studio Code
- SQL Server Management Studio (SSMS)


============================================================
3. PREREQUISITES
============================================================

Install the following before running the project:

1. .NET 8 SDK
2. Microsoft SQL Server
3. SQL Server Management Studio (SSMS)
4. Visual Studio 2022 with ASP.NET and web development workload
5. Git, if cloning the project from GitHub

Check the installed .NET version:

    dotnet --version

The project targets .NET 8.


============================================================
4. GET THE PROJECT
============================================================

Clone the repository:

    git clone <YOUR-GITHUB-REPOSITORY-URL>

Open the EventBridge solution/project in Visual Studio 2022.

IMPORTANT:
Do not commit or upload the real appsettings.json file.


============================================================
5. DATABASE SETUP
============================================================

EventBridge uses Microsoft SQL Server with Entity Framework Core.


5.1 START SQL SERVER
--------------------

Make sure the SQL Server service is running.

Open SQL Server Management Studio (SSMS) and connect to your
SQL Server instance.

The repository contains an Entity Framework Core Migrations folder,
so the database can normally be created/updated using the existing
migrations.


5.2 CONFIGURE THE CONNECTION STRING
------------------------------------

The repository provides:

    appsettings.example.json

Create a local copy and name it:

    appsettings.json

The real appsettings.json is intentionally excluded from GitHub
because it can contain:

- Database credentials
- Email credentials
- Razorpay credentials
- Other sensitive configuration

Configure the database connection string.

Windows Authentication example:

    Server=YOUR_SERVER_NAME;Database=EventBridgeDb;Trusted_Connection=True;TrustServerCertificate=True;

SQL Server Authentication example:

    Server=YOUR_SERVER_NAME;Database=EventBridgeDb;User Id=YOUR_USERNAME;Password=YOUR_PASSWORD;TrustServerCertificate=True;

Replace the placeholders with the SQL Server details on your machine.

IMPORTANT:
Do not copy these values blindly. Your SQL Server instance name
and authentication method may be different.


5.3 APPLY EXISTING EF CORE MIGRATIONS
-------------------------------------

The repository already contains the Migrations folder.

Using Visual Studio Package Manager Console:

    Update-Database

Or using the .NET CLI:

    dotnet ef database update

The existing migrations should normally be applied first.

Only create a new migration when you actually make a database/model
structure change.

Create a new migration using Visual Studio:

    Add-Migration <MigrationName>

Or using the CLI:

    dotnet ef migrations add <MigrationName>

Then apply it:

    Update-Database

or:

    dotnet ef database update

If the EF Core CLI tool is not installed:

    dotnet tool install --global dotnet-ef


============================================================
6. APPLICATION CONFIGURATION
============================================================

Create your local appsettings.json from:

    appsettings.example.json

The configuration contains:

- DefaultConnection
- MailSettings
- RazorpaySettings
- UploadSettings

Example structure:

    {
      "ConnectionStrings": {
        "DefaultConnection": "YOUR_DATABASE_CONNECTION_STRING"
      },

      "MailSettings": {
        "Mail": "YOUR_EMAIL_ADDRESS",
        "DisplayName": "EventBridge",
        "Password": "YOUR_EMAIL_APP_PASSWORD",
        "Host": "smtp.gmail.com",
        "Port": 587
      },

      "RazorpaySettings": {
        "KeyId": "YOUR_RAZORPAY_KEY_ID",
        "KeySecret": "YOUR_RAZORPAY_KEY_SECRET"
      },

      "UploadSettings": {
        "ProfileImagesPath": "wwwroot/uploads/profile",
        "PortfolioImagesPath": "wwwroot/uploads/portfolio",
        "MaxFileSizeMB": 5
      }
    }

Use your own credentials.


============================================================
7. EMAIL CONFIGURATION
============================================================

EventBridge uses MailSettings for email functionality.

Configure:

- Mail
- DisplayName
- Password
- Host
- Port

The project template uses:

    Host = smtp.gmail.com
    Port = 587

Use an appropriate email account/app password.

IMPORTANT:
Never commit the real email password or app password to GitHub.


============================================================
8. RAZORPAY CONFIGURATION
============================================================

EventBridge uses Razorpay for online booking payments.

Configure locally:

    "RazorpaySettings": {
      "KeyId": "YOUR_RAZORPAY_KEY_ID",
      "KeySecret": "YOUR_RAZORPAY_KEY_SECRET"
    }

Use the appropriate Razorpay credentials.

For development/testing, use Razorpay test credentials when applicable.

IMPORTANT:
Never publish the real Razorpay KeySecret.


============================================================
9. UPLOAD CONFIGURATION
============================================================

The project uses upload settings similar to:

    "UploadSettings": {
      "ProfileImagesPath": "wwwroot/uploads/profile",
      "PortfolioImagesPath": "wwwroot/uploads/portfolio",
      "MaxFileSizeMB": 5
    }

These settings control:

- Profile image upload location
- Portfolio image upload location
- Maximum configured upload size


============================================================
10. RESTORE DEPENDENCIES
============================================================

From the project directory:

    dotnet restore

Visual Studio can also restore NuGet packages automatically.


============================================================
11. BUILD THE PROJECT
============================================================

Using the .NET CLI:

    dotnet build

Or in Visual Studio:

    Build -> Build Solution


============================================================
12. RUN THE PROJECT
============================================================

Run the project from Visual Studio using its configured
HTTPS/IIS Express profile.

Alternatively:

    dotnet run

The exact localhost URL and port depend on the project's
launch settings.


============================================================
13. FIRST-RUN CHECKLIST
============================================================

Before running the application, make sure:

[ ] .NET 8 SDK is installed
[ ] SQL Server is installed and running
[ ] SSMS can connect to SQL Server
[ ] Local appsettings.json has been created
[ ] DefaultConnection is configured correctly
[ ] Existing EF Core migrations have been applied
[ ] Email settings are configured if email functionality is tested
[ ] Razorpay settings are configured if payment functionality is tested
[ ] Project builds successfully
[ ] Application starts successfully


============================================================
14. PROJECT STRUCTURE
============================================================

EventBridge
|
+-- Controllers
+-- Data
+-- Interfaces
+-- Migrations
+-- Models
+-- Repositories
+-- Services
+-- ViewModels
+-- Views
+-- wwwroot
+-- Program.cs
+-- EventBridge.csproj
+-- appsettings.example.json
+-- .gitignore
+-- README.md


============================================================
15. SECURITY
============================================================

Never commit the following:

- appsettings.json containing real credentials
- appsettings.Development.json containing real credentials
- .env files containing secrets
- Database passwords
- Email passwords/app passwords
- Razorpay secret keys
- Private API credentials

The repository should contain only the safe configuration template:

    appsettings.example.json

Anyone cloning the project should create their own local
appsettings.json and enter their own credentials.


============================================================
16. BEFORE MAKING THE GITHUB REPOSITORY PUBLIC
============================================================

Before changing the repository from Private to Public:

1. Confirm appsettings.json is NOT in the repository.

2. Confirm no real passwords are visible.

3. Confirm no real database credentials are visible.

4. Confirm no Razorpay KeySecret is visible.

5. Confirm no email app password is visible.

6. Confirm no log files contain sensitive information.

7. Confirm appsettings.example.json contains placeholders only.

8. Confirm the README contains proper setup instructions.

GitHub path:

    Repository -> Settings -> General
    -> Change repository visibility


IMPORTANT:
.gitignore prevents future accidental commits, but it does NOT
erase a secret that was already committed into Git history.

If a real credential was ever pushed to GitHub, rotate/change
that credential before making the repository public.


============================================================
17. IF A SECRET WAS EVER EXPOSED
============================================================

Treat the credential as compromised.

1. Change/rotate the credential immediately.
2. Remove the sensitive value from repository history if required.
3. Update the local configuration with the new credential.
4. Do not rely only on deleting the file from the latest commit.


============================================================
18. PAYMENT NOTES
============================================================

Razorpay is used for booking payments.

Keep Razorpay credentials in local configuration.

For development/testing, use the appropriate Razorpay test
environment and credentials when applicable.


============================================================
19. COMMON PROBLEMS
============================================================


DATABASE CONNECTION ERROR
-------------------------

Check:

- SQL Server is running.
- SQL Server instance/server name is correct.
- Authentication method is correct.
- DefaultConnection is correct.
- TrustServerCertificate settings if required.


DATABASE TABLES ARE MISSING
---------------------------

Make sure the migrations are present.

Run:

    dotnet ef database update

Or use:

    Update-Database

from Visual Studio Package Manager Console.


BUILD ERRORS AFTER CLONING
--------------------------

Run:

    dotnet restore

Then:

    dotnet build

Also confirm that a compatible .NET 8 SDK is installed.


PAYMENT NOT WORKING
-------------------

Check:

- Razorpay KeyId
- Razorpay KeySecret
- Test/live credentials
- Local appsettings.json


EMAIL NOT WORKING
-----------------

Check:

- MailSettings
- Email address
- Email/app password
- SMTP host
- SMTP port

Do not use another person's credentials.


UPLOADS NOT WORKING
-------------------

Check:

- UploadSettings paths
- Required upload directories
- Application write permissions


============================================================
20. DEVELOPMENT NOTES
============================================================

When database models are changed:

1. Modify the model.
2. Create a new EF Core migration.
3. Apply the migration.
4. Build the project.
5. Test the application.

Do not create migrations for changes that do not affect
the database schema.

Keep secrets in local configuration instead of source control.


============================================================
21. README
============================================================

The GitHub README should explain:

- What EventBridge is
- Main features
- Technology stack
- Project structure
- Main workflow
- Security considerations
- Payment integration
- Academic project context

The README provides the quick overview.

This setup guide provides the more detailed practical steps
needed to run the project locally.


============================================================
22. ACADEMIC PROJECT
============================================================

EventBridge was developed as an academic project to demonstrate
a web-based event planner marketplace using ASP.NET Core MVC,
Entity Framework Core, SQL Server, and Razor Views.


============================================================
END OF EVENTBRIDGE PROJECT SETUP GUIDE
============================================================
