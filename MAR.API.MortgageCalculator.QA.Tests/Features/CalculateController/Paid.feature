Feature: Paid
	/Calculate endpoint

@CalculateControllerTests
Scenario: Calculate (paid) endpoint returns successful response for request with HOA
	Given I want to call the API CalculateController '' resource
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
Scenario: Calculate (paid) endpoint returns successful response for request without HOA
	Given I want to call the API CalculateController '' resource
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