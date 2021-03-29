Feature: Paid
	/Calculate endpoint

@CalculateControllerTests @DeploymentSmokeTests
Scenario: Calculate (paid) endpoint returns successful response for request with HOA
	Given I issue an authorization token successfully to ScenarioContext
	And I want to call the API CalculateController '' resource
	And with the correct API authorization headers setup for an endpoint requiring authorization from ScenarioContext
	And with this API MortgageCalculationRequest
	| Field                  | Value     |
	| PurchasePrice          | 100000.00 |
	| APR                    | 2.75      |
	| LoanTermYears          | 30        |
	| DownPaymentPercent     | 20        |
	| PropertyTaxRate        | 1.15      |
	| HomeownerInsuranceRate | 0.22      |
	| HOAMonthly             | 10        |
	When I call the API using POST, the url, the headers and the request
	Then the API HTTP response is successful
	And the API HTTP domain response data is correct
	And the API HTTP response Data matches this successful MortgageCalculationResult
	| Field                             | Value    |
	| MortgagePaymentyMonthly           | 326.59   |
	| PropertyTaxPaymentMonthly         | 95.83    |
	| HomeownersInsurancePaymentMonthly | 18.33    |
	| DownPayment                       | 20000    |
	| TermInterestPaid                  | 37573.46 |

@CalculateControllerTests @DeploymentSmokeTests
Scenario: Calculate (paid) endpoint returns successful response for request without HOA
	Given I issue an authorization token successfully to ScenarioContext
	And I want to call the API CalculateController '' resource
	And with the correct API authorization headers setup for an endpoint requiring authorization from ScenarioContext
	And with this API MortgageCalculationRequest
	| Field                  | Value     |
	| PurchasePrice          | 100000.00 |
	| APR                    | 2.75      |
	| LoanTermYears          | 30        |
	| DownPaymentPercent     | 20        |
	| PropertyTaxRate        | 1.15      |
	| HomeownerInsuranceRate | 0.22      |
	| HOAMonthly             | 0         |
	When I call the API using POST, the url, the headers and the request
	Then the API HTTP response is successful
	And the API HTTP domain response data is correct
	And the API HTTP response Data matches this successful MortgageCalculationResult
	| Field                             | Value    |
	| MortgagePaymentyMonthly           | 326.59   |
	| PropertyTaxPaymentMonthly         | 95.83    |
	| HomeownersInsurancePaymentMonthly | 18.33    |
	| DownPayment                       | 20000    |
	| TermInterestPaid                  | 37573.46 |