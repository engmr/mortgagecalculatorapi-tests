Feature: Free
	/calculate/free

@CalculateControllerTests @DeploymentSmokeTests
Scenario: Calculate (free) endpoint returns successful response for request with HOA
	Given I want to call the API CalculateController 'free' resource
	And with this API MortgageCalculationRequest
	| Field                  | Value     |
	| PurchasePrice          | 100000.00 |
	| APR                    | 2.75      |
	| LoanTermYears          | 30        |
	| DownPaymentPercent     | 20        |
	| PropertyTaxRate        | 1.15      |
	| HomeownerInsuranceRate | 0.22      |
	| HOAMonthly             | 10        |
	When I call the API using POST, the url and the request
	Then the API HTTP response is successful
	And the API HTTP domain response data is correct
	And the API HTTP response Data matches this successful MortgageCalculationResult
	| Field                             | Value    |
	| MortgagePaymentyMonthly           | 326.59   |
	| PropertyTaxPaymentMonthly         | 95.83    |
	| HomeownersInsurancePaymentMonthly | 18.33    |
	| DownPayment                       | 20000    |
	| TermInterestPaid                  | 37573.46 |

@CalculateControllerTests
Scenario: Calculate (free) endpoint returns successful response for request without HOA
	Given I want to call the API CalculateController 'free' resource
	And with this API MortgageCalculationRequest
	| Field                  | Value     |
	| PurchasePrice          | 100000.00 |
	| APR                    | 2.75      |
	| LoanTermYears          | 30        |
	| DownPaymentPercent     | 20        |
	| PropertyTaxRate        | 1.15      |
	| HomeownerInsuranceRate | 0.22      |
	| HOAMonthly             | 0         |
	When I call the API using POST, the url and the request
	Then the API HTTP response is successful
	And the API HTTP domain response data is correct
	And the API HTTP response Data matches this successful MortgageCalculationResult
	| Field                             | Value    |
	| MortgagePaymentyMonthly           | 326.59   |
	| PropertyTaxPaymentMonthly         | 95.83    |
	| HomeownersInsurancePaymentMonthly | 18.33    |
	| DownPayment                       | 20000    |
	| TermInterestPaid                  | 37573.46 |

@CalculateControllerTests @RateLimitingTests
Scenario: Calculate (free) endpoint returns too many requests
	Given I want to call the API CalculateController 'free' resource
	And I am API rate limited to '2 per 10 s'
	And with this API MortgageCalculationRequest
	| Field                  | Value     |
	| PurchasePrice          | 100000.00 |
	| APR                    | 2.75      |
	| LoanTermYears          | 30        |
	| DownPaymentPercent     | 20        |
	| PropertyTaxRate        | 1.15      |
	| HomeownerInsuranceRate | 0.22      |
	| HOAMonthly             | 0         |
	When I call the API using POST, the url and the request
	Then the API HTTP response is successful
	When I call the API using POST, the url and the request
	Then the API HTTP response is successful
	When I call the API using POST, the url and the request
	Then the API HTTP response is too many requests
	And the API HTTP response Content is the API rate limited message from ScenarioContext
