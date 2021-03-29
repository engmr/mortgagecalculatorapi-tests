Feature: Issue
	Token issuing

@AuthorizeControllerTests @DeploymentSmokeTests
Scenario: Issue a token is successful
	Given I want to call the API AuthorizeController 'token/issue' resource
	And with the correct API authorization headers setup for issuing a token
	When I call the API using POST, the url and the headers
	Then the API HTTP response is successful
	And the API HTTP domain response data is correct
	And the API HTTP response Data is a valid Guid

@AuthorizeControllerTests @DeploymentSmokeTests
Scenario: Issue a token is not successful for invalid credentials
	Given I want to call the API AuthorizeController 'token/issue' resource
	And with the incorrect values for API authorization headers setup for issuing a token
	When I call the API using POST, the url and the headers
	Then the API HTTP response is unauthorized
	And the API HTTP domain response data is correct
	And the API HTTP response Data is null

@AuthorizeControllerTests
Scenario: Issue a token is not successful for invalid client id
	Given I want to call the API AuthorizeController 'token/issue' resource
	And with the incorrect client id for API authorization headers setup for issuing a token
	When I call the API using POST, the url and the headers
	Then the API HTTP response is unauthorized
	And the API HTTP domain response data is correct
	And the API HTTP response Data is null

@AuthorizeControllerTests
Scenario: Issue a token is not successful for invalid client password
	Given I want to call the API AuthorizeController 'token/issue' resource
	And with the incorrect client password for API authorization headers setup for issuing a token
	When I call the API using POST, the url and the headers
	Then the API HTTP response is unauthorized
	And the API HTTP domain response data is correct
	And the API HTTP response Data is null

@AuthorizeControllerTests
Scenario: Issue a token is not successful for missing client id
	Given I want to call the API AuthorizeController 'token/issue' resource
	And with the client id missing for API authorization headers setup for issuing a token
	When I call the API using POST, the url and the headers
	Then the API HTTP response is bad request
	And the API HTTP domain response data is correct
	And the API HTTP response Data is null

@AuthorizeControllerTests
Scenario: Issue a token is not successful for missing client password
	Given I want to call the API AuthorizeController 'token/issue' resource
	And with the client password missing for API authorization headers setup for issuing a token
	When I call the API using POST, the url and the headers
	Then the API HTTP response is bad request
	And the API HTTP domain response data is correct
	And the API HTTP response Data is null

@AuthorizeControllerTests
Scenario: Issue a token is not successful for passing empty values into headers
	Given I want to call the API AuthorizeController 'token/issue' resource
	And with empty valued API authorization headers setup for issuing a token
	When I call the API using POST, the url and the headers
	Then the API HTTP response is bad request
	And the API HTTP domain response data is correct
	And the API HTTP response Data is null

@AuthorizeControllerTests
Scenario: Issue a token is not successful for not passing any headers
	Given I want to call the API AuthorizeController 'token/issue' resource
	And with no API authorization headers setup for issuing a token
	When I call the API using POST and the url
	Then the API HTTP response is bad request
	And the API HTTP domain response data is correct
	And the API HTTP response Data is null

@AuthorizeControllerTests @RateLimitingTests
Scenario: Issue a token returns too many requests
	Given I want to call the API AuthorizeController 'token/issue' resource
	And with the correct API authorization headers setup for issuing a token
	And I am API rate limited to '15 per 5 s'
	When I call the API using POST, the url and the headers to trigger API rate limiting
	Then the API HTTP response is too many requests
	And the API HTTP response Content is the API rate limited message from ScenarioContext