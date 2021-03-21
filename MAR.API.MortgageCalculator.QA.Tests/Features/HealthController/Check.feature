Feature: Check
	Health check feature of the API

@HealthControllerTests
Scenario: Health Check endpoint returns successful
	Given I want to call the API HealthController 'check' resource
	When I call the API using GET and the url
	Then the API HTTP response is successful
	And the API HTTP domain response data is correct
	And the API HTTP response Data is null