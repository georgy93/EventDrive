Feature: Calculator
![Calculator](https://specflow.org/wp-content/uploads/2020/09/calculator.png)
Simple calculator for adding **two** numbers
Link to a feature: [Calculator](EventDrive.IntegrationTests/Features/Calculator.feature)
***Further read***: **[Learn more about how to generate Living Documentation](https://docs.specflow.org/projects/specflow-livingdoc/en/latest/LivingDocGenerator/Generating-Documentation.html)**

@mytag
Scenario: When I send a list of items to the WEB API and then send a synchronization request, the items should be found in the data store
	Given I have a list of items
	And I sent them to the web API
	When a request for synchronization event is sent
	Then the items should be found in the data store