Feature: Bulk
	As a consumer
	I want to be able to request multiple mortgage calculations in one request
	So that I can see multiple responses at once

@CalculateControllerTests @DeploymentSmokeTests
Scenario: Calculate (bulk) endpoint returns successful response for request with length 3
	Given I issue an authorization token successfully to ScenarioContext
	And I want to call the API CalculateController 'bulk' resource
	And with the correct API authorization headers setup for an endpoint requiring authorization from ScenarioContext
	And I have these API MortgageCalculationRequests for a bulk request
	| PurchasePrice | APR  | LoanTermYears | DownPaymentPercent | PropertyTaxRate | HomeownerInsuranceRate | HOAMonthly |
	| 100000.00     | 2.75 | 30            | 20                 | 1.15            | 0.22                   | 10         |
	| 120000.00     | 2.75 | 30            | 20                 | 1.15            | 0.22                   | 0          |
	| 100000.00     | 2.75 | 30            | 20                 | 1.15            | 0.22                   | 0          |
	And I have these API MortgageCalculationResults for a bulk request
	| MortgagePaymentyMonthly | PropertyTaxPaymentMonthly | HomeownersInsurancePaymentMonthly | DownPayment | TermInterestPaid |
	| 326.59                  | 95.83                     | 18.33                             | 20000       | 37573.46         |
	| 391.91                  | 115.00                    | 22.00                             | 24000       | 45088.15         |
	| 326.59                  | 95.83                     | 18.33                             | 20000       | 37573.46         |
	And I merged the API mortgage calculation Requests and Responses into the expected MortgageCalculationResults collection
	And I have created the API BulkMortgageCalculationRequest from ScenarioContext data
	When I call the API using POST, the url, the headers and the request
	Then the API HTTP response is successful
	And the API HTTP domain response data is correct
	And the API HTTP response Data matches the expected MortgageCalculationResults collection