Feature: Request Validation


Scenario: When I send request with no items I should receive BadRequest status code
	Given I have empty list of items
	When I sent them to the web API
	Then I should recieve BadRequest status code

Scenario: When I send request with item with empty Id I should receive BadRequest status code
	Given I have empty list of items containing an element with empty Id
	When I sent them to the web API
	Then I should recieve BadRequest status code

Scenario: When I send request with item with empty name I should receive BadRequest status code
	Given  I have empty list of items containing an element with empty Name
	When I sent them to the web API
	Then I should recieve BadRequest status code