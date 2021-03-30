using FluentAssertions;
using MAR.API.MortgageCalculator.QA.Tests.Model;
using System.Collections.Generic;
using System.Linq;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Xunit.Abstractions;

namespace MAR.API.MortgageCalculator.QA.Tests.Steps
{
    [Binding]
    public sealed class CalculateControllerSteps : BaseApiSteps
    {
        public CalculateControllerSteps(ITestOutputHelper testConsole, FeatureContext featureContext, ScenarioContext scenarioContext)
            : base(testConsole, featureContext, scenarioContext)
        {
        }

        [Given(@"I want to call the API CalculateController '(.*)' resource")]
        public void GivenIWantToCallTheAPICalculateControllerResource(string resourceInput)
        {
            SetAPIUrlFromControllerAndResource("calculate", resourceInput);
        }

        [Given(@"with this API MortgageCalculationRequest")]
        public void GivenWithThisAPIMortgageCalculationRequest(Table table)
        {
            var request = table.CreateInstance<MortgageCalculationRequest>();
            UpsertScenarioContextEntry(TestingSpecflowContextKeys.ApiRequestKey, request);
        }

        [Given(@"I have created the API BulkMortgageCalculationRequest from ScenarioContext data")]
        public void GivenIHaveCreatedTheAPIBulkMortgageCalculationRequestFromScenarioContextData()
        {
            var requests = GetScenarioContextItem<List<MortgageCalculationRequest>>(TestingSpecflowContextKeys.BulkRequestInputRequestsKey);
            requests.Should().NotBeNullOrEmpty();
            var request = new BulkMortgageCalcuationRequest()
            {
                Requests = requests
            };
            UpsertScenarioContextEntry(TestingSpecflowContextKeys.ApiRequestKey, request);
        }


        [Given(@"I have these API MortgageCalculationRequests for a bulk request")]
        public void GivenIHaveTheseAPIMortgageCalculationRequestsForABulkRequest(Table table)
        {
            var requests = table.CreateSet<MortgageCalculationRequest>().ToList();
            UpsertScenarioContextEntry(TestingSpecflowContextKeys.BulkRequestInputRequestsKey, requests);
        }

        [Given(@"I have these API MortgageCalculationResults for a bulk request")]
        public void GivenIHaveTheseAPIMortgageCalculationResultsForABulkRequest(Table table)
        {
            var responses = table.CreateSet<MortgageCalculationResult>().ToList();
            UpsertScenarioContextEntry(TestingSpecflowContextKeys.BulkRequestInputResponsesKey, responses);
        }

        /// <summary>
        /// Relies on input never being sorted from when it was input from the feature file
        /// </summary>
        [Given(@"I merged the API mortgage calculation Requests and Responses into the expected MortgageCalculationResults collection")]
        public void GivenIMergedTheAPIMortgageCalculationRequestsAndResponsesIntoTheExpectedMortgageCalculationResultsCollection()
        {
            var requests = GetScenarioContextItem<List<MortgageCalculationRequest>>(TestingSpecflowContextKeys.BulkRequestInputRequestsKey);
            var responses = GetScenarioContextItem<List<MortgageCalculationResult>>(TestingSpecflowContextKeys.BulkRequestInputResponsesKey);
            requests.Should().NotBeNullOrEmpty();
            responses.Should().NotBeNullOrEmpty();
            requests.Count.Should().Be(responses.Count);

            var mergedResponses = new List<MortgageCalculationResult>();
            for (int i = 0; i < requests.Count; i++)
            {
                var inputResponse = responses[i];
                mergedResponses.Add(new MortgageCalculationResult(requests[i])
                {
                    DownPayment = inputResponse.DownPayment,
                    HomeownersInsurancePaymentMonthly = inputResponse.HomeownersInsurancePaymentMonthly,
                    MortgagePaymentyMonthly = inputResponse.MortgagePaymentyMonthly,
                    PropertyTaxPaymentMonthly = inputResponse.PropertyTaxPaymentMonthly,
                    TermInterestPaid = inputResponse.TermInterestPaid
                });
            }
            UpsertScenarioContextEntry(TestingSpecflowContextKeys.ExpectedBulkResponseData, mergedResponses);
        }

        [Then(@"the API HTTP response Data matches this successful MortgageCalculationResult")]
        public void ThenTheAPIHTTPResponseDataMatchesThisSuccessfulMortgageCalculationResult(Table table)
        {
            var request = GetScenarioContextItem<MortgageCalculationRequest>(TestingSpecflowContextKeys.ApiRequestKey);
            var inputResponse = table.CreateInstance<MortgageCalculationResult>();
            var expectedResponse = new ApiResponse<MortgageCalculationResult>()
            {
                Data = new MortgageCalculationResult(request)
                {
                    DownPayment = inputResponse.DownPayment,
                    HomeownersInsurancePaymentMonthly = inputResponse.HomeownersInsurancePaymentMonthly,
                    MortgagePaymentyMonthly = inputResponse.MortgagePaymentyMonthly,
                    PropertyTaxPaymentMonthly = inputResponse.PropertyTaxPaymentMonthly,
                    TermInterestPaid = inputResponse.TermInterestPaid
                }
            };
            var actualResponse = GetMortgageCalculationResultApiResponseFromHttpResponseMessage();

            AssertThatResponseDataAreMatching(expectedResponse.Data, actualResponse.Data);
        }

        [Then(@"the API HTTP response Data matches the expected MortgageCalculationResults collection")]
        public void ThenTheAPIHTTPResponseDataMatchesTheExpectedMortgageCalculationResultsCollection()
        {
            var expectedResults = GetScenarioContextItem<List<MortgageCalculationResult>>(TestingSpecflowContextKeys.ExpectedBulkResponseData);
            expectedResults.Should().NotBeNull();
            var apiResponse = GetBulkMortgageCalculationResultApiResponseFromHttpResponseMessage();
            apiResponse.Should().NotBeNull();
            
            if (!expectedResults.Any())
            {
                apiResponse.Data.Should().BeEmpty();
            }
            else
            {
                foreach (var expectedResult in expectedResults)
                {
                    var actualResult = apiResponse.Data.SingleOrDefault(resp => resp.Request.RequestId == expectedResult.Request.RequestId);
                    actualResult.Should().NotBeNull();
                    AssertThatResponseDataAreMatching(expectedResult, actualResult);
                }
            }
        }

        private void AssertThatResponseDataAreMatching(MortgageCalculationResult expectedResult, MortgageCalculationResult actualResult)
        {
            //Request
            actualResult.Request.Should().NotBeNull();
            actualResult.Request.RequestId.Should().NotBeEmpty();
            actualResult.IsSuccessful.Should().Be(expectedResult.IsSuccessful);
            actualResult.Request.APR.Should().Be(expectedResult.Request.APR);
            actualResult.Request.DownPaymentPercent.Should().Be(expectedResult.Request.DownPaymentPercent);
            actualResult.Request.HOAMonthly.Should().Be(expectedResult.Request.HOAMonthly);
            actualResult.Request.HomeownerInsuranceRate.Should().Be(expectedResult.Request.HomeownerInsuranceRate);
            actualResult.Request.LoanTermYears.Should().Be(expectedResult.Request.LoanTermYears);
            actualResult.Request.PropertyTaxRate.Should().Be(expectedResult.Request.PropertyTaxRate);
            actualResult.Request.PurchasePrice.Should().Be(expectedResult.Request.PurchasePrice);

            //Calculation data
            actualResult.DownPayment.Should().Be(expectedResult.DownPayment);
            actualResult.Errors?.Count.Should().Be(expectedResult.Errors?.Count);
            foreach (var expectedError in expectedResult.Errors)
            {
                actualResult.Errors.SingleOrDefault(e => string.Equals(expectedError, e))
                    .Should().NotBeNull($"the error '{expectedError}' should be in the error list");
            }
            actualResult.HOAAnnual.Should().Be(actualResult.Request.HOAMonthly * 12, "annual should be 12x monthly");
            actualResult.HOAAnnual.Should().Be(expectedResult.HOAAnnual);
            actualResult.HomeownersInsurancePaymentMonthly.Should().Be(expectedResult.HomeownersInsurancePaymentMonthly);
            actualResult.MortgagePaymentyMonthly.Should().Be(expectedResult.MortgagePaymentyMonthly);
            actualResult.PropertyTaxPaymentMonthly.Should().Be(expectedResult.PropertyTaxPaymentMonthly);
            actualResult.TermInterestPaid.Should().Be(expectedResult.TermInterestPaid);
            actualResult.TotalCostOfLoan.Should().Be(actualResult.Request.PurchasePrice - actualResult.DownPayment + actualResult.TermInterestPaid);
            actualResult.TotalCostOfLoan.Should().Be(expectedResult.TotalCostOfLoan);
            actualResult.TotalMonthlyPayment.Should().Be(expectedResult.TotalMonthlyPayment);
            actualResult.ValidationErrors?.Count.Should().Be(expectedResult.ValidationErrors?.Count);
            foreach (var expectedError in expectedResult.ValidationErrors)
            {
                actualResult.ValidationErrors.SingleOrDefault(e => string.Equals(expectedError, e))
                    .Should().NotBeNull($"the error '{expectedError}' should be in the validation error list");
            }
        }
    }
}
