# mortgagecalculatorapi-tests
Integration tests for the MAR.API.MortgageCalculator API.

See https://github.com/engmr/mortgagecalculatorapi/blob/master/README.md for more information on this API.

## Prerequisites
1. .NET Core 5.0 or later installed
2. For Visual Studio, SpecFlow extension installed
3. (Until API model is in nuget) Cloned mortgagecalculatorpi repository for copying Model from.
4. ~~UserSecrets enabled on the MAR.API.MortgageCalculator.QA.Tests.csproj (see https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets?tabs=windows&view=aspnetcore-5.0#enable-secret-storage)~~ This is done for the project now.

## UserSecrets
Once the "Manage User Secrets" context menu option has been executed on the tests .csproj in Visual Studio ever on the machine, there will be a folder on the machine where UserSecrets are stored:  
> %APPDATA%\Microsoft\UserSecrets\<UserSecretsId from the .csproj>  
  
In here, the secrets.json file will be stored. Modifying existing entries (and saving) will override the appSettings.testing.json equivalent when the project is run.  
One of the main secrets is the AppSettings:BaseUrl. This allows a user to set any base url for testing the API without having to worry about accidentally checking it in.  
  
TIP: Right-click on the .csproj => "Manage User Secrets"; will open up the secrets.json file in VS.

### Add A New Secret to the secrets.json file
1. Save the secrets.json file
2. Add the new secret using the CLI command (while in .csproj directory)
   - e.g. `dotnet user-secrets set "AppSettings:MyNewKey" "SomeValue"`
3. Verify the secrets.json file was updated.  
  
A secret can also just be added manually to the secrets.json file.

#### Adding a New Secret to appSettings.testing.json
After the secret has been added to the machine:
1. Find the relevant appSettings.testing.json section
2. Add the key with value of null or a default value
   - e.g. "MyNewKey": null

### Example secrets.json file
```
{
  "AppSettings:BaseUrl": "http://localhost:9001/",
  "AppSettings:EmailPassword": "OneTwoThreeFourFive",
  "Database:Host": "http://localhost:55/"
}
```

## Test Execution
To run tests, these are the minimum entries needed in the secrets.json file (as of v 1.0.3):
```
{
  //"AppSettings:BaseUrl": "http://localhost:44363/",
  "AppSettings:BaseUrl": "yourUrlHere",
  "AppSettings:PublicPaidAccessUserId": "yourIdHere",
  "AppSettings:PublicPaidAccessUserPassword": "yourPasswordHere",
  "AppSettings:ApiRateLimitingXClientId": "yourClientIdEgTester1"
}
```

Once the secrets.json is edited/saved, open a cmd and navigate to the .csproj directory:
> cd c:\<yourGitRepo>\MAR.API.MortgageCalculator.QA.Tests  
  
and run all tests with:  
> dotnet test --verbosity normal

Tests can also be executed inside of Visual Studio.

### Test Specific Scenarios
SpecFlow tags are considered categories in `dotnet test`. See https://docs.specflow.org/projects/specflow/en/latest/Execution/Executing-Specific-Scenarios.html for more information.

Example (run only DeploymentSmokeTests):
> dotnet test --filter Category=DeploymentSmokeTests --verbosity normal
  
```
[xUnit.net 00:00:00.00] xUnit.net VSTest Adapter v2.4.3+1b45f5407b (64-bit .NET 5.0.4)
[xUnit.net 00:00:00.30]   Discovering: MAR.API.MortgageCalculator.QA.Tests
[xUnit.net 00:00:00.34]   Discovered:  MAR.API.MortgageCalculator.QA.Tests
[xUnit.net 00:00:00.34]   Starting:    MAR.API.MortgageCalculator.QA.Tests
  Passed Issue a token is not successful for invalid credentials [115 ms]
  Passed Token IsValid is not successful for invalid credentials [115 ms]
  Passed Health Check endpoint returns successful [115 ms]
  Passed Issue a token is successful [10 ms]
  Passed Token IsValid is successful [14 ms]
  Passed Calculate (paid) endpoint returns successful response for request with HOA [668 ms]
[xUnit.net 00:00:02.34]   Finished:    MAR.API.MortgageCalculator.QA.Tests

Test Run Successful.
Total tests: 6
     Passed: 6
 Total time: 2.7745 Seconds
     1>Done Building Project "C:\<yourGitRepo>\MAR.API.MortgageCalculator.QA.Tests\MAR.API.MortgageCalculator.QA.Tests.csproj" (VSTest target(s)).

Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:03.93
```
