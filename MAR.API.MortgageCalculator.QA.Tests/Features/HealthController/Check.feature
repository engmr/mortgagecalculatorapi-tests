Feature: Check
	Health check feature of the API

@HealthControllerTests
Scenario: Add two numbers
	Given I have the configured
	And the second number is 70
	When the two numbers are added
	Then the result should be 120