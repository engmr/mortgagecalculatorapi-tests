using FluentAssertions;
using MAR.API.MortgageCalculator.QA.Tests.Model;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using TechTalk.SpecFlow;
using Xunit.Abstractions;

namespace MAR.API.MortgageCalculator.QA.Tests.Steps
{
    public class BaseApiSteps
    {
        protected ITestOutputHelper TestConsole;
        protected FeatureContext TheFeatureContext;
        protected ScenarioContext TheScenarioContext;

        public BaseApiSteps(ITestOutputHelper testConsole, FeatureContext featureContext, ScenarioContext scenarioContext)
        {
            TestConsole = testConsole;
            TheFeatureContext = featureContext;
            TheScenarioContext = scenarioContext;
        }

        /// <summary>
        /// Sets API url from <see cref="AppSettings.BaseUrl"/> / method input; into the ScenarioContext
        /// </summary>
        /// <param name="controllerInput"></param>
        /// <param name="resourceInput"></param>
        protected void SetAPIUrlFromControllerAndResource(string controllerInput, string resourceInput)
        {
            var appSettings = GetAppSettings();
            appSettings.Should().NotBeNull();
            controllerInput.Should().NotBeNullOrWhiteSpace();
            resourceInput.Should().NotBeNullOrWhiteSpace();
            var sanitizedController = controllerInput
                .Replace(@"\", string.Empty)
                .Replace("/", string.Empty);
            var sanitizedResource = resourceInput.Replace(@"\", "/");
            sanitizedResource = sanitizedResource.IndexOf("/") == 0
                ? sanitizedResource[1..]
                : sanitizedResource;
            var apiUrl = Path.Combine(
                appSettings.BaseUrl,
                sanitizedController,
                sanitizedResource
                );
            var apiUri = new Uri(apiUrl.Replace(@"\", "/"));
            TestConsole.WriteLine("\t" + $"Full API Url: {apiUri.AbsoluteUri}");
            UpsertScenarioContextEntry(TestingContextKeys.ApiFullUrlKey, apiUri.AbsoluteUri);
        }

        /// <summary>
        /// Get FeatureContext AppSettings
        /// </summary>
        /// <returns></returns>
        protected AppSettings GetAppSettings()
        {
            return GetFeatureContextItem<AppSettings>(TestingContextKeys.AppSettingsKey);
        }

        /// <summary>
        /// Returns <see cref="ApiResponse{T}"/> from an <see cref="HttpResponseMessage"/>
        /// </summary>
        /// <returns></returns>
        protected ApiResponse<object> GetApiResponseFromHttpResponseMessage()
        {
            var apiHttpResponseMessage = GetScenarioContextItem<HttpResponseMessage>(TestingContextKeys.ApiResponseKey);
            apiHttpResponseMessage.Should().NotBeNull();
            var readTask = Task.Run(() => apiHttpResponseMessage.Content.ReadAsStringAsync());
            readTask.Wait(2000);
            var httpResponseJson = readTask.Result;
            httpResponseJson.Should().NotBeNullOrWhiteSpace();
            return JsonConvert.DeserializeObject<ApiResponse<object>>(httpResponseJson);
        }

        /// <summary>
        /// Add or update a ScenarioContext entry
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        protected void UpsertScenarioContextEntry(string key, object value)
        {
            if (TheScenarioContext.ContainsKey(key))
            {
                TheScenarioContext[key] = value;
            }
            else
            {
                TheScenarioContext.Add(key, value);
            }
        }

        /// <summary>
        /// Add or update a ScenarioContext entry
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        protected void UpsertFeatureContextEntry(string key, object value)
        {
            if (TheFeatureContext.ContainsKey(key))
            {
                TheFeatureContext[key] = value;
            }
            else
            {
                TheFeatureContext.Add(key, value);
            }
        }

        /// <summary>
        /// Get feature context item
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        protected T GetFeatureContextItem<T>(string key)
        {
            if (TheFeatureContext.ContainsKey(key))
            {
                return TheFeatureContext.Get<T>(key);
            }
            return default;
        }

        /// <summary>
        /// Get scenario context item
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        protected T GetScenarioContextItem<T>(string key)
        {
            if (TheScenarioContext.ContainsKey(key))
            {
                return TheScenarioContext.Get<T>(key);
            }
            return default;
        }
    }
}
