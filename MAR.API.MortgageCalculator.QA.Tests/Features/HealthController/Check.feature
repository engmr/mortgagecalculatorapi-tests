Feature: Check
	Health check feature of the API

@HealthControllerTests @DeploymentSmokeTests
Scenario: Health Check endpoint returns successful
	Given I want to call the API HealthController 'check' resource
	When I call the API using GET and the url
	Then the API HTTP response is successful
	And the API HTTP domain response data is correct
	And the API HTTP response Data is null

@HealthControllerTests @RateLimitingTests
Scenario: Health Check endpoint returns too many requests
	Given I want to call the API HealthController 'check' resource
	And I am API rate limited to '2 per 5 s'
	When I call the API using GET and the url to trigger API rate limiting
	Then the API HTTP response is too many requests
	And the API HTTP response Content is the API rate limited message from ScenarioContext