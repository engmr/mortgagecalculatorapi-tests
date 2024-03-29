﻿using FluentAssertions;
using MAR.API.MortgageCalculator.QA.Tests.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
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

        /// <summary>
        /// In ms.
        /// </summary>
        protected const int ApiCallTimeout = 15000;
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
            resourceInput.Should().NotBeNull();
            var sanitizedController = controllerInput
                .Trim()
                .Replace(@"\", string.Empty)
                .Replace("/", string.Empty);
            var sanitizedResource = resourceInput
                .Trim()
                .Replace(@"\", "/");
            sanitizedResource = sanitizedResource
                .IndexOf("/") == 0
                ? sanitizedResource[1..]
                : sanitizedResource;
            var apiUrl = Path.Combine(
                appSettings.BaseUrl,
                sanitizedController,
                sanitizedResource
                );
            var apiUri = new Uri(apiUrl.Replace(@"\", "/"));
            TestConsole.WriteLine("\t" + $"Full API Url: {apiUri.AbsoluteUri}");
            UpsertScenarioContextEntry(TestingSpecflowContextKeys.ApiFullUrlKey, apiUri.AbsoluteUri);
        }

        protected void AddApiRateLimitBypassClientIdHeaders()
        {
            var appSettings = GetAppSettings();
            var headers = new Dictionary<string, string>()
            {
                { TestingHeaderKeys.RateLimitXClientId, appSettings.ApiRateLimitingXClientId }
            };
            AddToOrCreateNewApiRequestHeadersCollectionToScenarioContext(headers);
        }

        protected void AddToOrCreateNewApiRequestHeadersCollectionToScenarioContext(Dictionary<string, string> headers)
        {
            headers.Should().NotBeNull();
            var apiRequestHeaders = GetScenarioContextItem<Dictionary<string, string>>(TestingSpecflowContextKeys.ApiRequestHeadersKey);
            if (apiRequestHeaders == null)
            {
                apiRequestHeaders = new Dictionary<string, string>(headers);
                UpsertScenarioContextEntry(TestingSpecflowContextKeys.ApiRequestHeadersKey, apiRequestHeaders);
                return;
            }
            foreach (var header in headers)
            {
                if (apiRequestHeaders.ContainsKey(header.Key))
                {
                    var warningMessage = $"Duplicate key found in {nameof(apiRequestHeaders)} for '{header.Key}'. Overwriting.";
                    TestConsole.WriteLine("\t" + warningMessage);
                    apiRequestHeaders[header.Key] = header.Value;
                }
                else
                {
                    apiRequestHeaders.Add(header.Key, header.Value);
                }
            }
            UpsertScenarioContextEntry(TestingSpecflowContextKeys.ApiRequestHeadersKey, apiRequestHeaders);
        }

        #region HttpClient Methods

        private Task<HttpResponseMessage> CallTheAPIUsingPOSTTheUrlAndTheHeaders(string apiUrl, Dictionary<string, string> apiRequestHeaders)
        {
            return new Task<HttpResponseMessage>(() =>
            {
                using (var httpClient = new HttpClient())
                {
                    foreach (var headerKVP in apiRequestHeaders)
                    {
                        httpClient.DefaultRequestHeaders.Add(headerKVP.Key, headerKVP.Value);
                    }

                    var postTask = Task.Run(() => httpClient.PostAsync(apiUrl, null));
                    var timedOut = !postTask.Wait(ApiCallTimeout);
                    if (!timedOut)
                    {
                        var response = postTask.Result;
                        return response;
                    }
                    throw new TimeoutException($"POST Request (with headers) to '{apiUrl}' timed out ({ApiCallTimeout} ms).");
                }
            });
        }

        protected void CallTheAPIUsingPOSTTheUrlAndTheHeaders()
        {
            var apiUrl = GetScenarioContextItem<string>(TestingSpecflowContextKeys.ApiFullUrlKey);
            apiUrl.Should().NotBeNullOrWhiteSpace();
            var apiRequestHeaders = GetScenarioContextItem<Dictionary<string, string>>(TestingSpecflowContextKeys.ApiRequestHeadersKey);
            apiRequestHeaders.Should().NotBeNull().And.NotBeEmpty();
            var apiRequestTask = CallTheAPIUsingPOSTTheUrlAndTheHeaders(apiUrl, apiRequestHeaders);
            apiRequestTask.Start();
            apiRequestTask.Wait();
            UpsertScenarioContextEntry(TestingSpecflowContextKeys.ApiResponseKey, apiRequestTask.Result);
        }

        protected void CallTheAPIUsingPOSTTheUrlAndTheHeadersToTriggerAPIRateLimiting()
        {
            var apiUrl = GetScenarioContextItem<string>(TestingSpecflowContextKeys.ApiFullUrlKey);
            apiUrl.Should().NotBeNullOrWhiteSpace();
            var apiRequestHeaders = GetScenarioContextItem<Dictionary<string, string>>(TestingSpecflowContextKeys.ApiRequestHeadersKey);
            apiRequestHeaders.Should().NotBeNull().And.NotBeEmpty();
            var apiRateLimitingTimeInterval = GetScenarioContextItem<ApiRateLimitingTimeInterval>(TestingSpecflowContextKeys.ApiRateLimitingTimeIntervalKey);
            apiRateLimitingTimeInterval.Should().NotBeNull();

            var numberOfCallsToTriggerApiRateLimiting = apiRateLimitingTimeInterval.RequestsAllowed + 1;
            var apiCallTasks = new List<Task<HttpResponseMessage>>();
            for (int i = 0; i < numberOfCallsToTriggerApiRateLimiting; i++)
            {
                apiCallTasks.Add(CallTheAPIUsingPOSTTheUrlAndTheHeaders(apiUrl, apiRequestHeaders));
            }
            apiCallTasks.ForEach(task => task.Start());
            Task.WaitAll(apiCallTasks.ToArray());
            var apiRateLimitedHttpResponse = apiCallTasks.FirstOrDefault(task => task.Result.StatusCode == HttpStatusCode.TooManyRequests)?.Result;
            if (apiRateLimitedHttpResponse == null)
            {
                TestConsole.WriteLine("\tNo rate limited response found");
                //Return last completed response
                apiRateLimitedHttpResponse = apiCallTasks.OrderByDescending(task => task.Result.Headers.Date.GetValueOrDefault()).First().Result;
            }
            UpsertScenarioContextEntry(TestingSpecflowContextKeys.ApiResponseKey, apiRateLimitedHttpResponse);
        }

        private Task<HttpResponseMessage> CallTheAPIUsingPOSTTheUrlAndTheRequest(string apiUrl, object apiRequest)
        {
            return new Task<HttpResponseMessage>(() =>
            {
                using (var httpClient = new HttpClient())
                {
                    var httpContent = new StringContent(JsonConvert.SerializeObject(apiRequest), Encoding.UTF8, "application/json");
                    var postTask = Task.Run(() => httpClient.PostAsync(apiUrl, httpContent));
                    var timedOut = !postTask.Wait(ApiCallTimeout);
                    if (!timedOut)
                    {
                        var response = postTask.Result;
                        return response;
                    }
                    throw new TimeoutException($"POST Request (with body) to '{apiUrl}' timed out ({ApiCallTimeout} ms).");
                }
            });
        }

        protected void CallTheAPIUsingPOSTTheUrlAndTheRequest()
        {
            var apiUrl = GetScenarioContextItem<string>(TestingSpecflowContextKeys.ApiFullUrlKey);
            apiUrl.Should().NotBeNullOrWhiteSpace();
            var apiRequest = GetScenarioContextItem<object>(TestingSpecflowContextKeys.ApiRequestKey);
            apiRequest.Should().NotBeNull();
            var apiRequestTask = CallTheAPIUsingPOSTTheUrlAndTheRequest(apiUrl, apiRequest);
            apiRequestTask.Start();
            apiRequestTask.Wait();
            UpsertScenarioContextEntry(TestingSpecflowContextKeys.ApiResponseKey, apiRequestTask.Result);
        }

        protected void CallTheAPIUsingPOSTTheUrlAndTheRequestToTriggerAPIRateLimiting()
        {
            var apiUrl = GetScenarioContextItem<string>(TestingSpecflowContextKeys.ApiFullUrlKey);
            apiUrl.Should().NotBeNullOrWhiteSpace();
            var apiRequest = GetScenarioContextItem<object>(TestingSpecflowContextKeys.ApiRequestKey);
            apiRequest.Should().NotBeNull();
            var apiRateLimitingTimeInterval = GetScenarioContextItem<ApiRateLimitingTimeInterval>(TestingSpecflowContextKeys.ApiRateLimitingTimeIntervalKey);
            apiRateLimitingTimeInterval.Should().NotBeNull();

            var numberOfCallsToTriggerApiRateLimiting = apiRateLimitingTimeInterval.RequestsAllowed + 1;
            var apiCallTasks = new List<Task<HttpResponseMessage>>();
            for (int i = 0; i < numberOfCallsToTriggerApiRateLimiting; i++)
            {
                apiCallTasks.Add(CallTheAPIUsingPOSTTheUrlAndTheRequest(apiUrl, apiRequest));
            }
            apiCallTasks.ForEach(task => task.Start());
            Task.WaitAll(apiCallTasks.ToArray());
            var apiRateLimitedHttpResponse = apiCallTasks.FirstOrDefault(task => task.Result.StatusCode == HttpStatusCode.TooManyRequests)?.Result;
            if (apiRateLimitedHttpResponse == null)
            {
                TestConsole.WriteLine("\tNo rate limited response found");
                //Return last completed response
                apiRateLimitedHttpResponse = apiCallTasks.OrderByDescending(task => task.Result.Headers.Date.GetValueOrDefault()).First().Result;
            }
            UpsertScenarioContextEntry(TestingSpecflowContextKeys.ApiResponseKey, apiRateLimitedHttpResponse);
        }

        private Task<HttpResponseMessage> CallTheAPIUsingPOSTTheUrlTheHeadersAndTheRequest(string apiUrl, object apiRequest, Dictionary<string, string> apiRequestHeaders)
        {
            return new Task<HttpResponseMessage>(() =>
            {
                using (var httpClient = new HttpClient())
                {
                    foreach (var headerKVP in apiRequestHeaders)
                    {
                        httpClient.DefaultRequestHeaders.Add(headerKVP.Key, headerKVP.Value);
                    }

                    var httpContent = new StringContent(JsonConvert.SerializeObject(apiRequest), Encoding.UTF8, "application/json");
                    var postTask = Task.Run(() => httpClient.PostAsync(apiUrl, httpContent));
                    var timedOut = !postTask.Wait(ApiCallTimeout);
                    if (!timedOut)
                    {
                        var response = postTask.Result;
                        return response;
                    }
                    throw new TimeoutException($"POST Request (with headers) to '{apiUrl}' timed out ({ApiCallTimeout} ms).");
                }
            });
        }

        protected void CallTheAPIUsingPOSTTheUrlTheHeadersAndTheRequest()
        {
            var apiUrl = GetScenarioContextItem<string>(TestingSpecflowContextKeys.ApiFullUrlKey);
            apiUrl.Should().NotBeNullOrWhiteSpace();
            var apiRequestHeaders = GetScenarioContextItem<Dictionary<string, string>>(TestingSpecflowContextKeys.ApiRequestHeadersKey);
            apiRequestHeaders.Should().NotBeNull().And.NotBeEmpty();
            var apiRequest = GetScenarioContextItem<object>(TestingSpecflowContextKeys.ApiRequestKey);
            apiRequest.Should().NotBeNull();
            var apiRequestTask = CallTheAPIUsingPOSTTheUrlTheHeadersAndTheRequest(apiUrl, apiRequest, apiRequestHeaders);
            apiRequestTask.Start();
            apiRequestTask.Wait();
            UpsertScenarioContextEntry(TestingSpecflowContextKeys.ApiResponseKey, apiRequestTask.Result);
        }

        protected void CallTheAPIUsingPOSTTheUrlTheHeadersAndTheRequestToTriggerAPIRateLimiting()
        {
            var apiUrl = GetScenarioContextItem<string>(TestingSpecflowContextKeys.ApiFullUrlKey);
            apiUrl.Should().NotBeNullOrWhiteSpace();
            var apiRequestHeaders = GetScenarioContextItem<Dictionary<string, string>>(TestingSpecflowContextKeys.ApiRequestHeadersKey);
            apiRequestHeaders.Should().NotBeNull().And.NotBeEmpty();
            var apiRequest = GetScenarioContextItem<object>(TestingSpecflowContextKeys.ApiRequestKey);
            apiRequest.Should().NotBeNull();
            var apiRateLimitingTimeInterval = GetScenarioContextItem<ApiRateLimitingTimeInterval>(TestingSpecflowContextKeys.ApiRateLimitingTimeIntervalKey);
            apiRateLimitingTimeInterval.Should().NotBeNull();

            var numberOfCallsToTriggerApiRateLimiting = apiRateLimitingTimeInterval.RequestsAllowed + 1;
            var apiCallTasks = new List<Task<HttpResponseMessage>>();
            for (int i = 0; i < numberOfCallsToTriggerApiRateLimiting; i++)
            {
                apiCallTasks.Add(CallTheAPIUsingPOSTTheUrlTheHeadersAndTheRequest(apiUrl, apiRequest, apiRequestHeaders));
            }
            apiCallTasks.ForEach(task => task.Start());
            Task.WaitAll(apiCallTasks.ToArray());
            var apiRateLimitedHttpResponse = apiCallTasks.FirstOrDefault(task => task.Result.StatusCode == HttpStatusCode.TooManyRequests)?.Result;
            if (apiRateLimitedHttpResponse == null)
            {
                TestConsole.WriteLine("\tNo rate limited response found");
                //Return last completed response
                apiRateLimitedHttpResponse = apiCallTasks.OrderByDescending(task => task.Result.Headers.Date.GetValueOrDefault()).First().Result;
            }
            UpsertScenarioContextEntry(TestingSpecflowContextKeys.ApiResponseKey, apiRateLimitedHttpResponse);
        }

        private Task<HttpResponseMessage> CallTheAPIUsingGETTheUrlAndTheHeaders(string apiUrl, Dictionary<string, string> apiRequestHeaders)
        {
            return new Task<HttpResponseMessage>(() =>
            {
                using (var httpClient = new HttpClient())
                {
                    foreach (var headerKVP in apiRequestHeaders)
                    {
                        httpClient.DefaultRequestHeaders.Add(headerKVP.Key, headerKVP.Value);
                    }

                    var getTask = Task.Run(() => httpClient.GetAsync(apiUrl));
                    var timedOut = !getTask.Wait(ApiCallTimeout);
                    if (!timedOut)
                    {
                        var response = getTask.Result;
                        return response;
                    }
                    throw new TimeoutException($"GET Request to '{apiUrl}' timed out ({ApiCallTimeout} ms).");
                }
            });
        }

        protected void CallTheAPIUsingGETTheUrlAndTheHeaders()
        {
            var apiUrl = GetScenarioContextItem<string>(TestingSpecflowContextKeys.ApiFullUrlKey);
            apiUrl.Should().NotBeNullOrWhiteSpace();
            var apiRequestHeaders = GetScenarioContextItem<Dictionary<string, string>>(TestingSpecflowContextKeys.ApiRequestHeadersKey);
            apiRequestHeaders.Should().NotBeNull().And.NotBeEmpty();
            var apiRequestTask = CallTheAPIUsingGETTheUrlAndTheHeaders(apiUrl, apiRequestHeaders);
            apiRequestTask.Start();
            apiRequestTask.Wait();
            UpsertScenarioContextEntry(TestingSpecflowContextKeys.ApiResponseKey, apiRequestTask.Result);
        }

        protected void CallTheAPIUsingGETTheUrlAndTheHeadersToTriggerAPIRateLimiting()
        {
            var apiUrl = GetScenarioContextItem<string>(TestingSpecflowContextKeys.ApiFullUrlKey);
            apiUrl.Should().NotBeNullOrWhiteSpace();
            var apiRequestHeaders = GetScenarioContextItem<Dictionary<string, string>>(TestingSpecflowContextKeys.ApiRequestHeadersKey);
            apiRequestHeaders.Should().NotBeNull().And.NotBeEmpty();
            var apiRateLimitingTimeInterval = GetScenarioContextItem<ApiRateLimitingTimeInterval>(TestingSpecflowContextKeys.ApiRateLimitingTimeIntervalKey);
            apiRateLimitingTimeInterval.Should().NotBeNull();

            var numberOfCallsToTriggerApiRateLimiting = apiRateLimitingTimeInterval.RequestsAllowed + 1;
            var apiCallTasks = new List<Task<HttpResponseMessage>>();
            for (int i = 0; i < numberOfCallsToTriggerApiRateLimiting; i++)
            {
                apiCallTasks.Add(CallTheAPIUsingGETTheUrlAndTheHeaders(apiUrl, apiRequestHeaders));
            }
            apiCallTasks.ForEach(task => task.Start());
            Task.WaitAll(apiCallTasks.ToArray());
            var apiRateLimitedHttpResponse = apiCallTasks.FirstOrDefault(task => task.Result.StatusCode == HttpStatusCode.TooManyRequests)?.Result;
            if (apiRateLimitedHttpResponse == null)
            {
                TestConsole.WriteLine("\tNo rate limited response found");
                //Return last completed response
                apiRateLimitedHttpResponse = apiCallTasks.OrderByDescending(task => task.Result.Headers.Date.GetValueOrDefault()).First().Result;
            }
            UpsertScenarioContextEntry(TestingSpecflowContextKeys.ApiResponseKey, apiRateLimitedHttpResponse);
        }

        private Task<HttpResponseMessage> CallTheAPIUsingGETAndTheUrl(string apiUrl)
        {
            return new Task<HttpResponseMessage>(() =>
            {
                using (var httpClient = new HttpClient())
                {
                    var getTask = Task.Run(() => httpClient.GetAsync(apiUrl));
                    var timedOut = !getTask.Wait(ApiCallTimeout);
                    if (!timedOut)
                    {
                        var response = getTask.Result;
                        return response;
                    }
                    throw new TimeoutException($"GET Request to '{apiUrl}' timed out ({ApiCallTimeout} ms).");
                }
            });
        }

        protected void CallTheAPIUsingGETAndTheUrl()
        {
            var apiUrl = GetScenarioContextItem<string>(TestingSpecflowContextKeys.ApiFullUrlKey);
            apiUrl.Should().NotBeNullOrWhiteSpace();
            var apiRequestTask = CallTheAPIUsingGETAndTheUrl(apiUrl);
            apiRequestTask.Start();
            apiRequestTask.Wait();
            UpsertScenarioContextEntry(TestingSpecflowContextKeys.ApiResponseKey, apiRequestTask.Result);
        }

        protected void CallTheAPIUsingGETAndTheUrlToTriggerAPIRateLimiting()
        {
            var apiUrl = GetScenarioContextItem<string>(TestingSpecflowContextKeys.ApiFullUrlKey);
            apiUrl.Should().NotBeNullOrWhiteSpace();
            var apiRateLimitingTimeInterval = GetScenarioContextItem<ApiRateLimitingTimeInterval>(TestingSpecflowContextKeys.ApiRateLimitingTimeIntervalKey);
            apiRateLimitingTimeInterval.Should().NotBeNull();

            var numberOfCallsToTriggerApiRateLimiting = apiRateLimitingTimeInterval.RequestsAllowed + 1;
            var apiCallTasks = new List<Task<HttpResponseMessage>>();
            for (int i = 0; i < numberOfCallsToTriggerApiRateLimiting; i++)
            {
                apiCallTasks.Add(CallTheAPIUsingGETAndTheUrl(apiUrl));
            }
            apiCallTasks.ForEach(task => task.Start());
            Task.WaitAll(apiCallTasks.ToArray());
            var apiRateLimitedHttpResponse = apiCallTasks.FirstOrDefault(task => task.Result.StatusCode == HttpStatusCode.TooManyRequests)?.Result;
            if (apiRateLimitedHttpResponse == null)
            {
                TestConsole.WriteLine("\tNo rate limited response found");
                //Return last completed response
                apiRateLimitedHttpResponse = apiCallTasks.OrderByDescending(task => task.Result.Headers.Date.GetValueOrDefault()).First().Result;
            }
            UpsertScenarioContextEntry(TestingSpecflowContextKeys.ApiResponseKey, apiRateLimitedHttpResponse);
        }

        private Task<HttpResponseMessage> CallTheAPIUsingPOSTAndTheUrl(string apiUrl)
        {
            return new Task<HttpResponseMessage>(() =>
            {
                using (var httpClient = new HttpClient())
                {
                    var postTask = Task.Run(() => httpClient.PostAsync(apiUrl, null));
                    var timedOut = !postTask.Wait(ApiCallTimeout);
                    if (!timedOut)
                    {
                        var response = postTask.Result;
                        return response;
                    }
                    throw new TimeoutException($"POST Request (no headers/body) to '{apiUrl}' timed out ({ApiCallTimeout} ms).");
                }
            });
        }

        protected void CallTheAPIUsingPOSTAndTheUrl()
        {
            var apiUrl = GetScenarioContextItem<string>(TestingSpecflowContextKeys.ApiFullUrlKey);
            apiUrl.Should().NotBeNullOrWhiteSpace();
            var apiRequestTask = CallTheAPIUsingPOSTAndTheUrl(apiUrl);
            apiRequestTask.Start();
            apiRequestTask.Wait();
            UpsertScenarioContextEntry(TestingSpecflowContextKeys.ApiResponseKey, apiRequestTask.Result);
        }

        protected void CallTheAPIUsingPOSTAndTheUrlToTriggerAPIRateLimiting()
        {
            var apiUrl = GetScenarioContextItem<string>(TestingSpecflowContextKeys.ApiFullUrlKey);
            apiUrl.Should().NotBeNullOrWhiteSpace();
            var apiRateLimitingTimeInterval = GetScenarioContextItem<ApiRateLimitingTimeInterval>(TestingSpecflowContextKeys.ApiRateLimitingTimeIntervalKey);
            apiRateLimitingTimeInterval.Should().NotBeNull();

            var numberOfCallsToTriggerApiRateLimiting = apiRateLimitingTimeInterval.RequestsAllowed + 1;
            var apiCallTasks = new List<Task<HttpResponseMessage>>();
            for (int i = 0; i < numberOfCallsToTriggerApiRateLimiting; i++)
            {
                apiCallTasks.Add(CallTheAPIUsingPOSTAndTheUrl(apiUrl));
            }
            apiCallTasks.ForEach(task => task.Start());
            Task.WaitAll(apiCallTasks.ToArray());
            var apiRateLimitedHttpResponse = apiCallTasks.FirstOrDefault(task => task.Result.StatusCode == HttpStatusCode.TooManyRequests)?.Result;
            if (apiRateLimitedHttpResponse == null)
            {
                TestConsole.WriteLine("\tNo rate limited response found");
                //Return last completed response
                apiRateLimitedHttpResponse = apiCallTasks.OrderByDescending(task => task.Result.Headers.Date.GetValueOrDefault()).First().Result;
            }
            UpsertScenarioContextEntry(TestingSpecflowContextKeys.ApiResponseKey, apiRateLimitedHttpResponse);
        }

        #endregion

        #region Assertion Methods
        protected void AssertTheAPIHttpResponseIsSuccessful()
        {
            var apiHttpResponseMessage = GetScenarioContextItem<HttpResponseMessage>(TestingSpecflowContextKeys.ApiResponseKey);
            apiHttpResponseMessage.Should().NotBeNull();
            TestConsole.WriteLine("\t" + $"API status code: {(int)apiHttpResponseMessage.StatusCode} ({apiHttpResponseMessage.StatusCode})");
            apiHttpResponseMessage.IsSuccessStatusCode.Should().BeTrue();
            apiHttpResponseMessage.Content.Should().NotBeNull();
        }

        protected void AssertTheAPIHttpResponseIsUnauthorized()
        {
            var apiHttpResponseMessage = GetScenarioContextItem<HttpResponseMessage>(TestingSpecflowContextKeys.ApiResponseKey);
            apiHttpResponseMessage.Should().NotBeNull();
            TestConsole.WriteLine("\t" + $"API status code: {(int)apiHttpResponseMessage.StatusCode} ({apiHttpResponseMessage.StatusCode})");
            apiHttpResponseMessage.IsSuccessStatusCode.Should().BeFalse();
            apiHttpResponseMessage.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            apiHttpResponseMessage.Content.Should().NotBeNull();
        }

        protected void AssertTheAPIHttpResponseIsBadRequest()
        {
            var apiHttpResponseMessage = GetScenarioContextItem<HttpResponseMessage>(TestingSpecflowContextKeys.ApiResponseKey);
            apiHttpResponseMessage.Should().NotBeNull();
            TestConsole.WriteLine("\t" + $"API status code: {(int)apiHttpResponseMessage.StatusCode} ({apiHttpResponseMessage.StatusCode})");
            apiHttpResponseMessage.IsSuccessStatusCode.Should().BeFalse();
            apiHttpResponseMessage.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            apiHttpResponseMessage.Content.Should().NotBeNull();
        }

        protected void AssertTheAPIHttpResponseIsTooManyRequests()
        {
            var apiHttpResponseMessage = GetScenarioContextItem<HttpResponseMessage>(TestingSpecflowContextKeys.ApiResponseKey);
            apiHttpResponseMessage.Should().NotBeNull();
            TestConsole.WriteLine("\t" + $"API status code: {(int)apiHttpResponseMessage.StatusCode} ({apiHttpResponseMessage.StatusCode})");
            apiHttpResponseMessage.IsSuccessStatusCode.Should().BeFalse();
            apiHttpResponseMessage.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
            apiHttpResponseMessage.Content.Should().NotBeNull();
        }

        protected void AssertTheAPIHttpHeadersShowRateLimitingWasApplied()
        {
            var apiHttpResponseMessage = GetScenarioContextItem<HttpResponseMessage>(TestingSpecflowContextKeys.ApiResponseKey);
            apiHttpResponseMessage.Should().NotBeNull();
            apiHttpResponseMessage.Headers.Should().NotBeNullOrEmpty();
            apiHttpResponseMessage.Headers.Contains(TestingHeaderKeys.RateLimitLimit).Should().BeTrue();
            apiHttpResponseMessage.Headers.Contains(TestingHeaderKeys.RateLimitRemaining).Should().BeTrue();
            apiHttpResponseMessage.Headers.Contains(TestingHeaderKeys.RateLimitReset).Should().BeTrue();
        }

        protected void AssertTheAPIHttpHeadersShowRateLimitingWasNotApplied()
        {
            var apiHttpResponseMessage = GetScenarioContextItem<HttpResponseMessage>(TestingSpecflowContextKeys.ApiResponseKey);
            apiHttpResponseMessage.Should().NotBeNull();
            apiHttpResponseMessage.Headers.Should().NotBeNullOrEmpty();
            apiHttpResponseMessage.Headers.Contains(TestingHeaderKeys.RateLimitLimit).Should().BeFalse();
            apiHttpResponseMessage.Headers.Contains(TestingHeaderKeys.RateLimitRemaining).Should().BeFalse();
            apiHttpResponseMessage.Headers.Contains(TestingHeaderKeys.RateLimitReset).Should().BeFalse();
        }

        protected void AssertTheAPIHttpDomainResponseDataIsCorrect()
        {
            var httpResponseResponse = GetApiResponseFromHttpResponseMessage();
            var appSettings = GetAppSettings();
            httpResponseResponse.ResponseDateTime.Should().NotBeSameDateAs(DateTime.MinValue);
            httpResponseResponse.APIVersion.Should().Be(appSettings.ApiResponseApiVersion);
            httpResponseResponse.ApplicationName.Should().Be(appSettings.ApiResponseApplicationName);
            httpResponseResponse.TransactionId.Should().NotBeEmpty();
        }

        protected void AssertTheAPIHttpResponseDataIsNull()
        {
            var httpResponseResponse = GetApiResponseFromHttpResponseMessage();
            httpResponseResponse.Data.Should().BeNull();
        }

        protected void AssertTheAPIHTTPResponseDataIsAValidGuid()
        {
            var responseDataString = GetTheAuthorizationTokenFromHttpResponseMessage();
            responseDataString.Should().NotBeNullOrWhiteSpace();
            Guid.TryParse(responseDataString, out Guid responseGuid).Should().BeTrue("the response Data string should be a Guid");
            responseGuid.Should().NotBeEmpty();
        }

        protected void AssertTheAPIHTTPResponseDataIsExpected(bool expected)
        {
            var httpResponseResponse = GetApiResponseFromHttpResponseMessage();
            var responseDataBool = httpResponseResponse.Data as bool?;
            responseDataBool.Should().HaveValue().And.Be(expected);
        }

        protected void AssertTheAPIHTTPResponseContentIsExpected(string expected)
        {
            var httpResponseResponse = GetStringContentFromHttpResponseMessage();
            httpResponseResponse.Should().Be(expected);
        }

        protected void AssertTheAPIHTTPResponseContentIsTheAPIRateLimitedMessage()
        {
            var timeInterval = GetScenarioContextItem<ApiRateLimitingTimeInterval>(TestingSpecflowContextKeys.ApiRateLimitingTimeIntervalKey);
            timeInterval.Should().NotBeNull();
            var expected = string.Format(TestingConstants.ApiRateLimitingResponseMessageFormat, timeInterval.GetApiRateLimitingResponseBodySuffix());
            AssertTheAPIHTTPResponseContentIsExpected(expected);
        } 
        #endregion

        protected string GetTheAuthorizationTokenFromHttpResponseMessage()
        {
            var httpResponseResponse = GetApiResponseFromHttpResponseMessage();
            return httpResponseResponse.Data as string;
        }

        protected void SetExpectedRateLimitingIntervalIntoScenarioContext(ApiRateLimitingTimeInterval timeInterval)
        {
            timeInterval.Should().NotBeNull();
            UpsertScenarioContextEntry(TestingSpecflowContextKeys.ApiRateLimitingTimeIntervalKey, timeInterval);
        }

        protected string GetAuthorizationTokenFromScenarioContext()
        {
            return GetScenarioContextItem<string>(TestingSpecflowContextKeys.AuthorizationTokenKey);
        }

        protected string GetAuthorizationTokenFromFeatureContext()
        {
            return GetFeatureContextItem<string>(TestingSpecflowContextKeys.AuthorizationTokenKey);
        }

        /// <summary>
        /// Get FeatureContext AppSettings
        /// </summary>
        /// <returns></returns>
        protected AppSettings GetAppSettings()
        {
            return GetFeatureContextItem<AppSettings>(TestingSpecflowContextKeys.AppSettingsKey);
        }

        /// <summary>
        /// Returns <see cref="ApiResponse{object}"/> from an <see cref="HttpResponseMessage"/>
        /// </summary>
        /// <returns></returns>
        protected ApiResponse<object> GetApiResponseFromHttpResponseMessage()
        {
            var apiHttpResponseMessage = GetScenarioContextItem<HttpResponseMessage>(TestingSpecflowContextKeys.ApiResponseKey);
            apiHttpResponseMessage.Should().NotBeNull();
            var readTask = Task.Run(() => apiHttpResponseMessage.Content.ReadAsStringAsync());
            readTask.Wait(2000);
            var httpResponseJson = readTask.Result;
            httpResponseJson.Should().NotBeNullOrWhiteSpace();
            return JsonConvert.DeserializeObject<ApiResponse<object>>(httpResponseJson);
        }

        /// <summary>
        /// Returns <see cref="string"/> from an <see cref="HttpResponseMessage"/>
        /// </summary>
        /// <returns></returns>
        protected string GetStringContentFromHttpResponseMessage()
        {
            var apiHttpResponseMessage = GetScenarioContextItem<HttpResponseMessage>(TestingSpecflowContextKeys.ApiResponseKey);
            apiHttpResponseMessage.Should().NotBeNull();
            var readTask = Task.Run(() => apiHttpResponseMessage.Content.ReadAsStringAsync());
            readTask.Wait(2000);
            var httpsResponseString = readTask.Result;
            httpsResponseString.Should().NotBeNullOrWhiteSpace();
            return httpsResponseString;
        }

        /// <summary>
        /// Returns <see cref="ApiResponse{MortgageCalculationResult}"/> from an <see cref="HttpResponseMessage"/>
        /// </summary>
        /// <returns></returns>
        protected ApiResponse<MortgageCalculationResult> GetMortgageCalculationResultApiResponseFromHttpResponseMessage()
        {
            var apiHttpResponseMessage = GetScenarioContextItem<HttpResponseMessage>(TestingSpecflowContextKeys.ApiResponseKey);
            apiHttpResponseMessage.Should().NotBeNull();
            var readTask = Task.Run(() => apiHttpResponseMessage.Content.ReadAsStringAsync());
            readTask.Wait(2000);
            var httpResponseJson = readTask.Result;
            httpResponseJson.Should().NotBeNullOrEmpty();

            return JsonConvert.DeserializeObject<ApiResponse<MortgageCalculationResult>>(httpResponseJson);
        }

        /// <summary>
        /// Returns <see cref="ApiResponse{List<MortgageCalculationResult>}"/> from an <see cref="HttpResponseMessage"/>
        /// </summary>
        /// <returns></returns>
        protected ApiResponse<List<MortgageCalculationResult>> GetBulkMortgageCalculationResultApiResponseFromHttpResponseMessage()
        {
            var apiHttpResponseMessage = GetScenarioContextItem<HttpResponseMessage>(TestingSpecflowContextKeys.ApiResponseKey);
            apiHttpResponseMessage.Should().NotBeNull();
            var readTask = Task.Run(() => apiHttpResponseMessage.Content.ReadAsStringAsync());
            readTask.Wait(2000);
            var httpResponseJson = readTask.Result;
            httpResponseJson.Should().NotBeNullOrEmpty();

            return JsonConvert.DeserializeObject<ApiResponse<List<MortgageCalculationResult>>>(httpResponseJson);
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
