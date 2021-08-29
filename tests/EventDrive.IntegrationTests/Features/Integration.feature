Feature: Integration

Scenario: When I send a list of items to the WEB API and then send a synchronization request, the items should be found in the data store
	Given I have a list of items
	And I sent them to the web API
	When a request for synchronization event is sent
	Then the items should be found in the data store