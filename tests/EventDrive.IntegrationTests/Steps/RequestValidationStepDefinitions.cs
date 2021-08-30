namespace EventDrive.IntegrationTests.Steps
{
    using Common;
    using DTOs;
    using DTOs.Commands;
    using FluentAssertions;
    using Refit;
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Threading.Tasks;
    using TechTalk.SpecFlow;

    [Binding]
    public class RequestValidationStepDefinitions
    {
        private readonly IEventDriveAPIClient _eventDriveAPI;

        private IReadOnlyCollection<MyDTO> _listOfDtos;
        private Task _apiResultTask;

        public RequestValidationStepDefinitions(IEventDriveAPIClient eventDriveAPI)
        {
            _eventDriveAPI = eventDriveAPI;
        }

        [Given(@"I have empty list of items")]
        public void GivenIHaveEmptyListOfItems()
        {
            _listOfDtos = Array.Empty<MyDTO>();
        }

        [Given(@"I have empty list of items containing an element with empty Id")]
        public void GivenIHaveEmptyListOfItemsContainingAnElementWithEmptyId()
        {
            _listOfDtos = new List<MyDTO>()
            {
                new() { Id = Guid.NewGuid().ToString(), Name = string.Empty },
                new() { Id = Guid.NewGuid().ToString(), Name = "khjavgsduh" }
            };
        }

        [Given(@"I have empty list of items containing an element with empty Name")]
        public void GivenIHaveEmptyListOfItemsContainingAnElementWithEmptyName()
        {
            _listOfDtos = new List<MyDTO>()
            {
                new() { Id = string.Empty, Name = "dsavdvgf" },
                new() { Id = Guid.NewGuid().ToString(), Name = "bvfdbtrb" }
            };
        }

        [When(@"I sent them to the web API")]
        public void WhenISentThemToTheWebAPI()
        {
            _apiResultTask = _eventDriveAPI.AddItemsToRedisAsync(new AddItemsCommand(_listOfDtos));
        }

        [Then(@"I should recieve BadRequest status code")]
        public async Task ThenIShouldRecieveBadRequestStatusCode()
        {
            try
            {
                await _apiResultTask;
            }
            catch (ApiException ex)
            {
                ex.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }
    }
}