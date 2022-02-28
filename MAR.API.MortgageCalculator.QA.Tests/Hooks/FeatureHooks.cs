using Microsoft.Extensions.Configuration;
using TechTalk.SpecFlow;

namespace MAR.API.MortgageCalculator.QA.Tests.Hooks
{    
    [Binding]
    public static class TestRunHooks
    {
        private static TestRunContext _testRunContext;

        [BeforeTestRun]
        public static void SetupTestRun(IObjectContainer testRunObjectContainer)
        {
            KillAllChromeDrivers();
            _testRunContext = new TestRunContext();
            ReadInAppSettings(_testRunContext);
        }

        [BeforeFeature]
        public static void SetupFeature(IObjectContainer testRunObjectContainer)
        {
            testRunObjectContainer.RegisterInstanceAs<ITestRunContext>(_testRunContext);
        }

        [BeforeScenario]
        public static void Setup(IObjectContainer testRunObjectContainer, ITestOutputHelper testConsole, ITestRunContext testRunContext)
        {
            var webBrowserType = testRunContext.AppSettings?.WebBrowserType;
            if (webBrowserType == null)
            {
                testConsole.WriteLine("WARNING: Web driver browser type was not set in test run context. Defaulting to Edge");
                webBrowserType = WebDriverBrowserTypes.Edge;
            }
            var webDriverWrapper = new WebDriverWrapper();
            webDriverWrapper.Initialize(webBrowserType.Value);
            testRunObjectContainer.RegisterInstanceAs<IWebDriverWrapper>(webDriverWrapper);
        }

        [AfterScenario]
        public static void TearDown(IObjectContainer testRunObjectContainer)
        {
            testRunObjectContainer.Resolve<IWebDriverWrapper>()?.Quit();
        }

        [AfterTestRun]
        public static void Teardown()
        {
            KillAllChromeDrivers();
        }

        private static void KillAllChromeDrivers()
        {
            foreach (var process in Process.GetProcessesByName("chromedriver"))
            {
                process.Kill();
            }
        }

        private static void ReadInAppSettings(TestRunContext testRunContext)
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appSettings.testing.json", optional: false, reloadOnChange: false)
#if DEBUG
                .AddUserSecrets("d4bbeecf-3610-487a-b6c7-ebd95cc0ad5c")
#endif
                .Build();

            var appSettings = new AppSettings();
            config.GetSection("AppSettings").Bind(appSettings);
            testRunContext.AppSettings = appSettings;
        }
    }

    public class TestRunContext : ITestRunContext
    {
        public AppSettings AppSettings { get; set; }
    }

    public interface ITestRunContext
    {
        AppSettings AppSettings { get; set; }
    }
}
