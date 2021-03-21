using Microsoft.Extensions.Configuration;
using TechTalk.SpecFlow;

namespace MAR.API.MortgageCalculator.QA.Tests.Hooks
{
    [Binding]
    public class FeatureHooks
    {
        [BeforeFeature]
        public static void ReadInAppSettings(FeatureContext featureContext)
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appSettings.testing.json", optional: false, reloadOnChange: true)
                .AddUserSecrets("d4bbeecf-3610-487a-b6c7-ebd95cc0ad5c")
                .Build();
            var appSettings = new AppSettings();
            config.GetSection("AppSettings").Bind(appSettings);
            featureContext.Add(TestingContextKeys.AppSettingsKey, appSettings);
        }
    }
}
