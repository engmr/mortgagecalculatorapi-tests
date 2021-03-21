using FluentAssertions;
using MAR.API.MortgageCalculator.QA.Tests.Model;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
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
                var timedOut = !getTask.Wait(ApiCallTimeout);
                if (!timedOut)
                {
                    var response = getTask.Result;
                    UpsertScenarioContextEntry(TestingContextKeys.ApiResponseKey, response);
                    return;
                }
                throw new TimeoutException($"GET Request to '{apiUrl}' timed out ({ApiCallTimeout} ms).");
            }
        }

        [When(@"I call the API using POST, the url and the request")]
        public void WhenICallTheAPIUsingPOSTTheUrlAndTheRequest()
        {
            var apiUrl = GetScenarioContextItem<string>(TestingContextKeys.ApiFullUrlKey);
            apiUrl.Should().NotBeNullOrWhiteSpace();
            var apiRequest = GetScenarioContextItem<MortgageCalculationRequest>(TestingContextKeys.ApiRequestKey);
            apiRequest.Should().NotBeNull();
            using (var httpClient = new HttpClient())
            {
                var httpContent = new StringContent(JsonConvert.SerializeObject(apiRequest), Encoding.UTF8, "application/json");
                var postTask = Task.Run(() => httpClient.PostAsync(apiUrl, httpContent));
                var timedOut = !postTask.Wait(ApiCallTimeout);
                if (!timedOut)
                {
                    var response = postTask.Result;
                    UpsertScenarioContextEntry(TestingContextKeys.ApiResponseKey, response);
                    return;
                }
                throw new TimeoutException($"POST Request to '{apiUrl}' timed out ({ApiCallTimeout} ms).");
            }
        }

        [Then(@"the API HTTP response is successful")]
        public void ThenTheAPIHttpResponseIsSuccessful()
        {
            var apiHttpResponseMessage = GetScenarioContextItem<HttpResponseMessage>(TestingContextKeys.ApiResponseKey);
            apiHttpResponseMessage.Should().NotBeNull();
            TestConsole.WriteLine("\t" + $"API status code: {(int)apiHttpResponseMessage.StatusCode} ({apiHttpResponseMessage.StatusCode})");
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
