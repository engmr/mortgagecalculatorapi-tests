Feature: Check
	Health check feature of the API

@HealthControllerTests @DeploymentSmokeTests
Scenario: Health Check endpoint returns successful
	Given I want to call the API HealthController 'check' resource
	And with the ClientId bypass API rate limiting header added to the headers
	When I call the API using GET, the url and the headers
	Then the API HTTP response is successful
	And the API HTTP domain response data is correct
	And the API HTTP response Data is null

@HealthControllerTests @RateLimitingTests
Scenario: Health Check endpoint returns successful response when bypassing API rate limiting using ClientWhiteList
	Given I want to call the API HealthController 'check' resource
	And with the ClientId bypass API rate limiting header added to the headers
	And I am API rate limited to '2 per 5 s'
	When I call the API using GET, the url and the headers to trigger API rate limiting
	Then the API HTTP response is successful
	And the API HTTP headers indicate that API rate limiting was not applied
	And the API HTTP domain response data is correct
	And the API HTTP response Data is null

@HealthControllerTests @RateLimitingTests
Scenario: Health Check endpoint returns too many requests
	Given I want to call the API HealthController 'check' resource
	And I am API rate limited to '2 per 5 s'
	When I call the API using GET and the url to trigger API rate limiting
	Then the API HTTP response is too many requests
	And the API HTTP response Content is the API rate limited message from ScenarioContext