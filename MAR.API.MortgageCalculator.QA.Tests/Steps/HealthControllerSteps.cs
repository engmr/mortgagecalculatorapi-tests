using TechTalk.SpecFlow;
using Xunit.Abstractions;

namespace MAR.API.MortgageCalculator.QA.Tests.Steps
{
    [Binding]
    public sealed class HealthControllerSteps : BaseApiSteps
    {
        public HealthControllerSteps(ITestOutputHelper testConsole, FeatureContext featureContext, ScenarioContext scenarioContext)
            : base(testConsole, featureContext, scenarioContext)
        {
        }

        [Given(@"I want to call the API HealthController '(.*)' resource")]
        public void GivenIWantToCallTheAPIHealthControllerResource(string resourceInput)
        {
            SetAPIUrlFromControllerAndResource("health", resourceInput);
        }
    }
}
