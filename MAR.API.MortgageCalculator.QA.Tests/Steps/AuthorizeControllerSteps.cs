using FluentAssertions;
using System;
using System.Collections.Generic;
using TechTalk.SpecFlow;
using Xunit.Abstractions;

namespace MAR.API.MortgageCalculator.QA.Tests.Steps
{
    [Binding]
    public sealed class AuthorizeControllerSteps : BaseApiSteps
    {
        private Guid _fakeClientId;
        private string _fakeClientPassword;
        private string _fakeToken;

        public AuthorizeControllerSteps(ITestOutputHelper testConsole, FeatureContext featureContext, ScenarioContext scenarioContext)
            : base(testConsole, featureContext, scenarioContext)
        {
            _fakeClientId = Guid.Parse("49f825a8-c0b2-4998-a7b4-9a43f31b4d97");
            _fakeClientPassword = "f773fc2f68f89919b94bdf21077c3f09";
            _fakeToken = "ddbd3b02-0082-4664-8120-e00711e7883c";
        }

        [Given(@"I issue an authorization token successfully to ScenarioContext")]
        public void GivenIIssueAnAuthorizationTokenSuccessfullyToScenarioContext()
        {
            GivenIWantToCallTheAPIAuthorizeControllerResource("token/issue");
            GivenWithTheCorrectAPIAuthorizationHeadersSetupForIssuingAToken();
            CallTheAPIUsingPOSTTheUrlAndTheHeaders();
            AssertTheAPIHttpResponseIsSuccessful();
            var authToken = GetTheAuthorizationTokenFromHttpResponseMessage();
            authToken.Should().NotBeNullOrWhiteSpace();
            UpsertScenarioContextEntry(TestingSpecflowContextKeys.AuthorizationTokenKey, authToken);
        }

        [Given(@"I issue an authorization token successfully to FeatureContext")]
        public void GivenIIssueAnAuthorizationTokenSuccessfullyToFeatureContext()
        {
            GivenIWantToCallTheAPIAuthorizeControllerResource("token/issue");
            GivenWithTheCorrectAPIAuthorizationHeadersSetupForIssuingAToken();
            CallTheAPIUsingPOSTTheUrlAndTheHeaders();
            AssertTheAPIHttpResponseIsSuccessful();
            var authToken = GetTheAuthorizationTokenFromHttpResponseMessage();
            authToken.Should().NotBeNullOrWhiteSpace();
            UpsertFeatureContextEntry(TestingSpecflowContextKeys.AuthorizationTokenKey, authToken);
        }

        [Given(@"I want to call the API AuthorizeController '(.*)' resource")]
        public void GivenIWantToCallTheAPIAuthorizeControllerResource(string resourceInput)
        {
            SetAPIUrlFromControllerAndResource("authorize", resourceInput);
        }

        [Given(@"with the correct API authorization headers setup for issuing a token")]
        public void GivenWithTheCorrectAPIAuthorizationHeadersSetupForIssuingAToken()
        {
            var appSettings = GetAppSettings();
            var headers = new Dictionary<string, string>()
            {
                { TestingHeaderKeys.ClientId, appSettings.PublicPaidAccessUserId.ToString() },
                { TestingHeaderKeys.Password, appSettings.PublicPaidAccessUserPassword },
            };
            UpsertScenarioContextEntry(TestingSpecflowContextKeys.ApiRequestHeadersKey, headers);
        }

        [Given(@"with the incorrect values for API authorization headers setup for issuing a token")]
        public void GivenWithTheIncorrectValuesForAPIAuthorizationHeadersSetupForIssuingAToken()
        {
            var headers = new Dictionary<string, string>()
            {
                { TestingHeaderKeys.ClientId, _fakeClientId.ToString() },
                { TestingHeaderKeys.Password, _fakeClientPassword },
            };
            UpsertScenarioContextEntry(TestingSpecflowContextKeys.ApiRequestHeadersKey, headers);
        }

        [Given(@"with the incorrect client id for API authorization headers setup for issuing a token")]
        public void GivenWithTheIncorrectClientIdForAPIAuthorizationHeadersSetupForIssuingAToken()
        {
            var appSettings = GetAppSettings();
            var headers = new Dictionary<string, string>()
            {
                { TestingHeaderKeys.ClientId, _fakeClientId.ToString() },
                { TestingHeaderKeys.Password, appSettings.PublicPaidAccessUserPassword },
            };
            UpsertScenarioContextEntry(TestingSpecflowContextKeys.ApiRequestHeadersKey, headers);
        }

        [Given(@"with the incorrect client password for API authorization headers setup for issuing a token")]
        public void GivenWithTheIncorrectClientPasswordForAPIAuthorizationHeadersSetupForIssuingAToken()
        {
            var appSettings = GetAppSettings();
            var headers = new Dictionary<string, string>()
            {
                { TestingHeaderKeys.ClientId, appSettings.PublicPaidAccessUserId.ToString() },
                { TestingHeaderKeys.Password, _fakeClientPassword },
            };
            UpsertScenarioContextEntry(TestingSpecflowContextKeys.ApiRequestHeadersKey, headers);
        }

        [Given(@"with the client id missing for API authorization headers setup for issuing a token")]
        public void GivenWithTheClientIdMissingForAPIAuthorizationHeadersSetupForIssuingAToken()
        {
            var headers = new Dictionary<string, string>()
            {
                { TestingHeaderKeys.Password, _fakeClientPassword },
            };
            UpsertScenarioContextEntry(TestingSpecflowContextKeys.ApiRequestHeadersKey, headers);
        }

        [Given(@"with the client password missing for API authorization headers setup for issuing a token")]
        public void GivenWithTheClientPasswordMissingForAPIAuthorizationHeadersSetupForIssuingAToken()
        {
            var headers = new Dictionary<string, string>()
            {
                { TestingHeaderKeys.ClientId, _fakeClientId.ToString() },
            };
            UpsertScenarioContextEntry(TestingSpecflowContextKeys.ApiRequestHeadersKey, headers);
        }

        [Given(@"with no API authorization headers setup for issuing a token")]
        [Given(@"with no API authorization headers setup for an endpoint requiring authorization")]
        public void GivenWithNoAPIAuthorizationHeadersSetupForIssuingAToken()
        {
            var headers = new Dictionary<string, string>();
            UpsertScenarioContextEntry(TestingSpecflowContextKeys.ApiRequestHeadersKey, headers);
        }

        [Given(@"with empty valued API authorization headers setup for issuing a token")]
        public void GivenWithEmptyValuedAPIAuthorizationHeadersSetupForIssuingAToken()
        {
            var headers = new Dictionary<string, string>()
            {
                { TestingHeaderKeys.ClientId, string.Empty },
                { TestingHeaderKeys.Password, string.Empty },
            };
            UpsertScenarioContextEntry(TestingSpecflowContextKeys.ApiRequestHeadersKey, headers);
        }

        [Given(@"with the correct API authorization headers setup for an endpoint requiring authorization from ScenarioContext")]
        public void GivenWithTheCorrectAPIAuthorizationHeadersSetupForAnEndpointRequiringAuthorizationFromScenarioContext()
        {
            var appSettings = GetAppSettings();
            var headers = new Dictionary<string, string>()
            {
                { TestingHeaderKeys.ClientId, appSettings.PublicPaidAccessUserId.ToString() },
                { TestingHeaderKeys.AuthorizationToken, GetAuthorizationTokenFromScenarioContext() },
            };
            UpsertScenarioContextEntry(TestingSpecflowContextKeys.ApiRequestHeadersKey, headers);
        }

        [Given(@"with the correct API authorization headers setup for an endpoint requiring authorization from FeatureContext")]
        public void GivenWithTheCorrectAPIAuthorizationHeadersSetupForAnEndpointRequiringAuthorizationFromFeatureContext()
        {
            var appSettings = GetAppSettings();
            var headers = new Dictionary<string, string>()
            {
                { TestingHeaderKeys.ClientId, appSettings.PublicPaidAccessUserId.ToString() },
                { TestingHeaderKeys.AuthorizationToken, GetAuthorizationTokenFromFeatureContext() },
            };
            UpsertScenarioContextEntry(TestingSpecflowContextKeys.ApiRequestHeadersKey, headers);
        }

        [Given(@"with the incorrect values for API authorization headers setup for an endpoint requiring authorization")]
        public void GivenWithTheIncorrectValuesForAPIAuthorizationHeadersSetupForAnEndpointRequiringAuthorization()
        {
            var headers = new Dictionary<string, string>()
            {
                { TestingHeaderKeys.ClientId, _fakeClientId.ToString() },
                { TestingHeaderKeys.AuthorizationToken, _fakeToken },
            };
            UpsertScenarioContextEntry(TestingSpecflowContextKeys.ApiRequestHeadersKey, headers);
        }

        [Given(@"with the incorrect client id for API authorization headers setup for an endpoint requiring authorization from ScenarioContext")]
        public void GivenWithTheIncorrectClientIdForAPIAuthorizationHeadersSetupForAnEndpointRequiringAuthorizationFromScenarioContext()
        {
            var appSettings = GetAppSettings();
            var headers = new Dictionary<string, string>()
            {
                { TestingHeaderKeys.ClientId, _fakeClientId.ToString() },
                { TestingHeaderKeys.AuthorizationToken, GetAuthorizationTokenFromScenarioContext() },
            };
            UpsertScenarioContextEntry(TestingSpecflowContextKeys.ApiRequestHeadersKey, headers);
        }

        [Given(@"with the incorrect client id for API authorization headers setup for an endpoint requiring authorization from FeatureContext")]
        public void GivenWithTheIncorrectClientIdForAPIAuthorizationHeadersSetupForAnEndpointRequiringAuthorizationFromFeatureContext()
        {
            var appSettings = GetAppSettings();
            var headers = new Dictionary<string, string>()
            {
                { TestingHeaderKeys.ClientId, _fakeClientId.ToString() },
                { TestingHeaderKeys.AuthorizationToken, GetAuthorizationTokenFromFeatureContext() },
            };
            UpsertScenarioContextEntry(TestingSpecflowContextKeys.ApiRequestHeadersKey, headers);
        }

        [Given(@"with the incorrect authorization token for API authorization headers setup for an endpoint requiring authorization")]
        public void GivenWithTheIncorrectAuthorizationTokenForAPIAuthorizationHeadersSetupForAnEndpointRequiringAuthorization()
        {
            var appSettings = GetAppSettings();
            var headers = new Dictionary<string, string>()
            {
                { TestingHeaderKeys.ClientId, appSettings.PublicPaidAccessUserId.ToString() },
                { TestingHeaderKeys.AuthorizationToken, _fakeToken },
            };
            UpsertScenarioContextEntry(TestingSpecflowContextKeys.ApiRequestHeadersKey, headers);
        }

        [Given(@"with the client id missing for API authorization headers setup for an endpoint requiring authorization")]
        public void GivenWithTheClientIdMissingForAPIAuthorizationHeadersSetupForAnEndpointRequiringAuthorization()
        {
            var headers = new Dictionary<string, string>()
            {
                { TestingHeaderKeys.AuthorizationToken, _fakeToken },
            };
            UpsertScenarioContextEntry(TestingSpecflowContextKeys.ApiRequestHeadersKey, headers);
        }

        [Given(@"with the authorization token missing for API authorization headers setup for an endpoint requiring authorization")]
        public void GivenWithTheAuthorizationTokenMissingForAPIAuthorizationHeadersSetupForAnEndpointRequiringAuthorization()
        {
            var headers = new Dictionary<string, string>()
            {
                { TestingHeaderKeys.ClientId, _fakeClientId.ToString() },
            };
            UpsertScenarioContextEntry(TestingSpecflowContextKeys.ApiRequestHeadersKey, headers);
        }

        [Given(@"with empty valued API authorization headers setup for an endpoint requiring authorization")]
        public void GivenWithEmptyValuedAPIAuthorizationHeadersSetupForAnEndpointRequiringAuthorization()
        {
            var headers = new Dictionary<string, string>()
            {
                { TestingHeaderKeys.ClientId, string.Empty },
                { TestingHeaderKeys.AuthorizationToken, string.Empty },
            };
            UpsertScenarioContextEntry(TestingSpecflowContextKeys.ApiRequestHeadersKey, headers);
        }
    }
}
