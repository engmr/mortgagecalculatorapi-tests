# mortgagecalculatorapi-tests
Integration tests for the MAR.API.MortgageCalculator API.

See https://github.com/engmr/mortgagecalculatorapi/blob/master/README.md for more information on this API.

## Prerequisites
1. .NET Core 5.0 or later installed
2. For Visual Studio, SpecFlow extension installed
3. (Until API model is in nuget) Cloned mortgagecalculatorpi repository for copying Model from.
4. ~~UserSecrets enabled on the MAR.API.MortgageCalculator.QA.Tests.csproj (see https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets?tabs=windows&view=aspnetcore-5.0#enable-secret-storage)~~ This is done for the project now.

## UserSecrets
Once the first secret has ever been added on a machine (https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets?tabs=windows&view=aspnetcore-5.0#set-a-secret), there will be a folder on the machine where UserSecrets are stored:  
> %APPDATA%\Microsoft\UserSecrets\<UserSecretsId from the .csproj>  
In here, the secrets.json file will be stored. Modifying existing entries (and saving) will override the appSettings.testing.json equivalent when the project is run.  
One of the main secrets is the AppSettings:BaseUrl. This allows a user to set any base url for testing the API without having to worry about accidentally checking it in.  
  
TIP: Right-click on the .csproj => "Manage User Secrets"; will open up the secrets.json file in VS.

### Add A New Secret to the Machine
1. Save the secrets.json file
2. Add the new secret using the CLI command (while in .csproj directory)
   - e.g. `dotnet user-secrets set "AppSettings:MyNewKey" "SomeValue"`
3. Verify the secrets.json file was updated.

#### Adding a New Secret to appSettings.testing.json
After the secret has been added to the machine:
1. Find the relevant appSettings.testing.json section
2. Add the key with value of null or a default value
   - e.g. "MyNewKey": null

### Example secrets.json file
```
{
  "AppSettings:BaseUrl": "http://localhost:9001/",
  "AppSettings:EmailPassword": "OneTwoThreeFourFive"
}
```