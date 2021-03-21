using FluentAssertions;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using TechTalk.SpecFlow;
using Xunit.Abstractions;

namespace MAR.API.MortgageCalculator.QA.Tests.Steps
{
    [Binding]
    public sealed class CommonApiSteps : BaseApiSteps
    {
        public CommonApiSteps(ITestOutputHelper testConsole, FeatureContext featureContext, ScenarioContext scenarioContext)
            : base(testConsole, featureContext, scenarioContext)
        {
        }

        [When(@"I call the API using GET and the url")]
        public void WhenICallTheAPIUsingGETAndTheUrl()
        {
            var apiUrl = GetScenarioContextItem<string>(TestingContextKeys.ApiFullUrlKey);
            apiUrl.Should().NotBeNullOrWhiteSpace();
            using (var httpClient = new HttpClient())
            {
                var getTask = Task.Run(() => httpClient.GetAsync(apiUrl));
                getTask.Wait(15000);
                var response = getTask.Result;
                UpsertScenarioContextEntry(TestingContextKeys.ApiResponseKey, response);
            }
        }

        [Then(@"the API HTTP response is successful")]
        public void ThenTheAPIHttpResponseIsSuccessful()
        {
            var apiHttpResponseMessage = GetScenarioContextItem<HttpResponseMessage>(TestingContextKeys.ApiResponseKey);
            apiHttpResponseMessage.Should().NotBeNull();
            apiHttpResponseMessage.IsSuccessStatusCode.Should().BeTrue();
            apiHttpResponseMessage.Content.Should().NotBeNull();
        }

        [Then(@"the API HTTP domain response data is correct")]
        public void ThenTheAPIHttpDomainResponseDataIsCorrect()
        {
            var httpResponseResponse = GetApiResponseFromHttpResponseMessage();
            var appSettings = GetAppSettings();
            httpResponseResponse.ResponseDateTime.Should().NotBeSameDateAs(DateTime.MinValue);
            httpResponseResponse.APIVersion.Should().Be(appSettings.ApiResponseApiVersion);
            httpResponseResponse.ApplicationName.Should().Be(appSettings.ApiResponseApplicationName);
            httpResponseResponse.TransactionId.Should().NotBeEmpty();
        }

        [Then(@"the API HTTP response Data is null")]
        public void ThenTheAPIHttpResponseDataIsNull()
        {
            var httpResponseResponse = GetApiResponseFromHttpResponseMessage();
            httpResponseResponse.Data.Should().BeNull();
        }

    }
}
