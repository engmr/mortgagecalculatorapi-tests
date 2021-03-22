﻿Feature: IsValid
	Check if a token is valid

@AuthorizeControllerTests @DeploymentSmokeTests
Scenario: Token IsValid is successful
	Given I issue an authorization token successfully to ScenarioContext
	And I want to call the API AuthorizeController 'token/isvalid' resource
	And with the correct API authorization headers setup for an endpoint requiring authorization from ScenarioContext
	When I call the API using GET, the url and the headers
	Then the API HTTP response is successful
	And the API HTTP domain response data is correct
	And the API HTTP response Data is bool 'true'

@AuthorizeControllerTests @DeploymentSmokeTests
Scenario: Token IsValid is not successful for invalid credentials
	Given I want to call the API AuthorizeController 'token/isvalid' resource
	And with the incorrect values for API authorization headers setup for an endpoint requiring authorization
	When I call the API using GET, the url and the headers
	Then the API HTTP response is successful
	And the API HTTP domain response data is correct
	And the API HTTP response Data is bool 'false'

@AuthorizeControllerTests
Scenario: Token IsValid is not successful for invalid client id
	Given I issue an authorization token successfully to ScenarioContext
	And I want to call the API AuthorizeController 'token/isvalid' resource
	And with the incorrect client id for API authorization headers setup for an endpoint requiring authorization from ScenarioContext
	When I call the API using GET, the url and the headers
	Then the API HTTP response is successful
	And the API HTTP domain response data is correct
	And the API HTTP response Data is bool 'false'

@AuthorizeControllerTests
Scenario: Token IsValid is not successful for invalid authorization token
	Given I want to call the API AuthorizeController 'token/isvalid' resource
	And with the incorrect authorization token for API authorization headers setup for an endpoint requiring authorization
	When I call the API using GET, the url and the headers
	Then the API HTTP response is successful
	And the API HTTP domain response data is correct
	And the API HTTP response Data is bool 'false'

@AuthorizeControllerTests
Scenario: Token IsValid is not successful for missing client id
	Given I want to call the API AuthorizeController 'token/isvalid' resource
	And with the client id missing for API authorization headers setup for an endpoint requiring authorization
	When I call the API using GET, the url and the headers
	Then the API HTTP response is bad request
	And the API HTTP domain response data is correct
	And the API HTTP response Data is null

@AuthorizeControllerTests
Scenario: Token IsValid is not successful for missing authorization token
	Given I want to call the API AuthorizeController 'token/isvalid' resource
	And with the authorization token missing for API authorization headers setup for an endpoint requiring authorization
	When I call the API using GET, the url and the headers
	Then the API HTTP response is bad request
	And the API HTTP domain response data is correct
	And the API HTTP response Data is null

@AuthorizeControllerTests
Scenario: Token IsValid is not successful for passing empty values into headers
	Given I want to call the API AuthorizeController 'token/isvalid' resource
	And with empty valued API authorization headers setup for an endpoint requiring authorization
	When I call the API using GET, the url and the headers
	Then the API HTTP response is bad request
	And the API HTTP domain response data is correct
	And the API HTTP response Data is null

@AuthorizeControllerTests
Scenario: Token IsValid is not successful for not passing any headers
	Given I want to call the API AuthorizeController 'token/isvalid' resource
	And with no API authorization headers setup for an endpoint requiring authorization
	When I call the API using GET and the url
	Then the API HTTP response is bad request
	And the API HTTP domain response data is correct
	And the API HTTP response Data is null