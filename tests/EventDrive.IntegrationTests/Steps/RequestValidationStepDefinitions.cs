namespace EventDrive.IntegrationTests.Steps;

using Common;
using DTOs;
using DTOs.Commands;
using Refit;
using Reqnroll;
using System.Net;
using Xunit;

[Binding]
public class RequestValidationStepDefinitions
{
    private readonly IEventDriveApiClient _eventDriveAPI;

    private IReadOnlyCollection<MyDto> _listOfDtos;
    private Task _apiResultTask;

    public RequestValidationStepDefinitions(IEventDriveApiClient eventDriveAPI)
    {
        _eventDriveAPI = eventDriveAPI;
    }

    [Given(@"I have empty list of items")]
    public void GivenIHaveEmptyListOfItems()
    {
        _listOfDtos = [];
    }

    [Given(@"I have empty list of items containing an element with empty Id")]
    public void GivenIHaveEmptyListOfItemsContainingAnElementWithEmptyId()
    {
        _listOfDtos =
        [
            new() { Id = string.Empty, Name = "dsavdvgf" },
            new() { Id = Guid.NewGuid().ToString(), Name = "khjavgsduh" }
        ];
    }

    [Given(@"I have empty list of items containing an element with empty Name")]
    public void GivenIHaveEmptyListOfItemsContainingAnElementWithEmptyName()
    {
        _listOfDtos =
        [
            new() { Id = Guid.NewGuid().ToString(), Name = string.Empty },
            new() { Id = Guid.NewGuid().ToString(), Name = "bvfdbtrb" }
        ];
    }

    [When(@"I sent them to the web API")]
    public void WhenISentThemToTheWebAPI()
    {
        _apiResultTask = _eventDriveAPI.AddItemsToRedisAsync(new AddItemsCommand(_listOfDtos));
    }

    [Then(@"I should recieve BadRequest status code")]
    public async Task ThenIShouldRecieveBadRequestStatusCode()
    {
        var exception = await Assert.ThrowsAsync<ApiException>(async () => await _apiResultTask);

        Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);
    }
}