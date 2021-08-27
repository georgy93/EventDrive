namespace EventDrive.IntegrationTests.Steps
{
    using DTOs.Commands;
    using EventDrive.DTOs;
    using EventDrive.IntegrationTests.Common;
    using FluentAssertions;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using TechTalk.SpecFlow;
    using Xunit;

    [Binding]
    public sealed class IntegrationStepDefinitions
    {
        private readonly IEventDriveAPIClient _eventDriveAPI;

        private IReadOnlyCollection<MyDTO> _listOfDtos;

        public IntegrationStepDefinitions(IEventDriveAPIClient eventDriveAPI)
        {
            _eventDriveAPI = eventDriveAPI;
        }

        [Given(@"I have a list of items")]
        public void GivenIHaveAListOfItems()
        {
            _listOfDtos = new List<MyDTO>()
            {
                new() { Id = Guid.NewGuid().ToString() },
                new() { Id = Guid.NewGuid().ToString() },
                new() { Id = Guid.NewGuid().ToString() }
            };
        }

        [Given(@"I sent them to the web API")]
        public async Task GivenISentThemToTheWebAPI()
        {
            await _eventDriveAPI.AddItemsToRedisAsync(new AddItemsCommand(_listOfDtos));
        }

        [When(@"a request for synchronization event is sent")]
        public async Task WhenARequestForSynchronizationEventIsSent()
        {
            await _eventDriveAPI.NotifyItemsAddedAsync();
        }

        [Then(@"the items should be found in the data store")]
        public void ThenTheItemsShouldBeFoundInTheDataStore()
        {
            // Arrange
            var expectedResult = _listOfDtos.Select(x => x.Id).ToList();
            var actualResult = new List<string>();
            // call db and get result

            // Assert
            actualResult.Should().BeEquivalentTo(expectedResult);
        }
    }
}