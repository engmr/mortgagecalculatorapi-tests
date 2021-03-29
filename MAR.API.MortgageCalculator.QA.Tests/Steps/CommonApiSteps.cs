using MAR.API.MortgageCalculator.QA.Tests.Model;
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
            CallTheAPIUsingGETAndTheUrl();
        }

        [When(@"I call the API using POST, the url and the request")]
        public void WhenICallTheAPIUsingPOSTTheUrlAndTheRequest()
        {
            CallTheAPIUsingPOSTTheUrlAndTheRequest();
        }

        [Given(@"I call the API using POST, the url and the headers")]
        [When(@"I call the API using POST, the url and the headers")]
        public void WhenICallTheAPIUsingPOSTTheUrlAndTheHeaders()
        {
            CallTheAPIUsingPOSTTheUrlAndTheHeaders();
        }

        [Given(@"I call the API using POST, the url, the headers and the request")]
        [When(@"I call the API using POST, the url, the headers and the request")]
        public void WhenICallTheAPIUsingPOSTTheUrlTheHeadersAndTheRequest()
        {
            CallTheAPIUsingPOSTTheUrlTheHeadersAndTheRequest();
        }

        [Given(@"I call the API using GET, the url and the headers")]
        [When(@"I call the API using GET, the url and the headers")]
        public void WhenICallTheAPIUsingGETTheUrlAndTheHeaders()
        {
            CallTheAPIUsingGETTheUrlAndTheHeaders();
        }

        [When(@"I call the API using POST and the url")]
        public void WhenICallTheAPIUsingPOSTAndTheUrl()
        {
            CallTheAPIUsingPOSTAndTheUrl();
        }


        [Given(@"the API HTTP response is successful")]
        [Then(@"the API HTTP response is successful")]
        public void ThenTheAPIHttpResponseIsSuccessful()
        {
            AssertTheAPIHttpResponseIsSuccessful();
        }

        [Then(@"the API HTTP response is unauthorized")]
        public void ThenTheAPIHttpResponseIsUnauthorized()
        {
            AssertTheAPIHttpResponseIsUnauthorized();
        }

        [Then(@"the API HTTP response is bad request")]
        public void ThenTheAPIHttpResponseIsBadRequest()
        {
            AssertTheAPIHttpResponseIsBadRequest();
        }

        [Then(@"the API HTTP response is too many requests")]
        public void ThenTheAPIHttpResponseIsTooManyRequests()
        {
            AssertTheAPIHttpResponseIsTooManyRequests();
        }

        [Then(@"the API HTTP domain response data is correct")]
        public void ThenTheAPIHttpDomainResponseDataIsCorrect()
        {
            AssertTheAPIHttpDomainResponseDataIsCorrect();
        }

        [Then(@"the API HTTP response Data is null")]
        public void ThenTheAPIHttpResponseDataIsNull()
        {
            AssertTheAPIHttpResponseDataIsNull();
        }

        [Given(@"the API HTTP response Data is a valid Guid")]
        [Then(@"the API HTTP response Data is a valid Guid")]
        public void ThenTheAPIHTTPResponseDataIsAValidGuid()
        {
            AssertTheAPIHTTPResponseDataIsAValidGuid();
        }

        [Then(@"the API HTTP response Data is bool '(.*)'")]
        public void ThenTheAPIHttpResponseDataIs(bool expected)
        {
            AssertTheAPIHTTPResponseDataIsExpected(expected);
        }

        [Then(@"the API HTTP response Content is string '(.*)'")]
        public void ThenTheAPIHTTPResponseContentIsString(string expected)
        {
            AssertTheAPIHTTPResponseContentIsExpected(expected);
        }

        [Then(@"the API HTTP response Content is the API rate limited message from ScenarioContext")]
        public void ThenTheAPIHTTPResponseContentIsTheAPIRateLimitedMessageFromScenarioContext()
        {
            AssertTheAPIHTTPResponseContentIsTheAPIRateLimitedMessage();
        }

        [Given(@"I am API rate limited to '(.*)'")]
        public void GivenIAmAPIRateLimitedTo(ApiRateLimitingTimeInterval rateLimitedTimeSpan)
        {
            SetExpectedRateLimitingIntervalIntoScenarioContext(rateLimitedTimeSpan);
        }

        [When(@"I call the API using POST, the url and the request to trigger API rate limiting")]
        public void WhenICallTheAPIUsingPOSTTheUrlAndTheRequestToTriggerAPIRateLimiting()
        {
            CallTheAPIUsingPOSTTheUrlAndTheRequestToTriggerAPIRateLimiting();
        }

    }
}
