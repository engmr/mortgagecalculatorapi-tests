using TechTalk.SpecFlow;

namespace MAR.API.MortgageCalculator.QA.Tests.Steps
{
    [Binding]
    public sealed class HealthControllerSteps : BaseApiSteps
    {
        public HealthControllerSteps(ScenarioContext scenarioContext)
            : base(scenarioContext)
        {
        }

        [Given("the first number is (.*)")]
        public void GivenTheFirstNumberIs(int number)
        {
            //TODO: implement arrange (precondition) logic
            // For storing and retrieving scenario-specific data see https://go.specflow.org/doc-sharingdata
            // To use the multiline text or the table argument of the scenario,
            // additional string/Table parameters can be defined on the step definition
            // method. 

            TheScenarioContext.Pending();
        }
    }
}
