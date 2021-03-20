using TechTalk.SpecFlow;

namespace MAR.API.MortgageCalculator.QA.Tests.Steps
{
    [Binding]
    public class BaseApiSteps
    {
        protected ScenarioContext TheScenarioContext;
        public BaseApiSteps(ScenarioContext scenarioContext)
        {
            TheScenarioContext = scenarioContext;
        }
    }
}
