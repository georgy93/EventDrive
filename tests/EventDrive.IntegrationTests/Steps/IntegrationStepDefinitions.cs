namespace EventDrive.IntegrationTests.Steps;

using Common;
using DTOs;
using DTOs.Commands;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Reqnroll;
using Xunit;

[Binding]
public sealed class IntegrationStepDefinitions
{
    private readonly string _dbConnectionString;
    private readonly IEventDriveApiClient _eventDriveAPI;

    private IReadOnlyCollection<MyDto> _listOfDtos;

    public IntegrationStepDefinitions(IConfiguration configuration, IEventDriveApiClient eventDriveAPI)
    {
        _dbConnectionString = configuration.GetSection("EventDriveDBConnectionString").Value;
        _eventDriveAPI = eventDriveAPI;
    }

    [Given(@"I have a list of items")]
    public void GivenIHaveAListOfItems()
    {
        _listOfDtos = Enumerable
            .Range(0, 3)
            .Select(x =>
            {
                var guid = Guid.NewGuid().ToString();

                return new MyDto
                {
                    Id = guid,
                    Name = guid + "lala"
                };
            })
            .ToList();
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
    public async Task ThenTheItemsShouldBeFoundInTheDataStore()
    {
        // wait some time for the worker to insert data.
        // Other option is to poll the database for the items or use SQLDependency for change notification
        await Task.Delay(TimeSpan.FromSeconds(6));

        // Arrange
        var expectedResult = _listOfDtos.Select(x => x.Id).ToList();

        // Act
        var actualResult = await GetDataFromDataStoreAsync();

        // Assert
        foreach (var id in expectedResult)
        {
            Assert.Contains(id, actualResult);
        }
    }

    private async Task<IEnumerable<string>> GetDataFromDataStoreAsync()
    {
        using var connection = new SqlConnection(_dbConnectionString);

        await connection.OpenAsync();

        var command = new SqlCommand("SELECT * FROM dbo.Items", connection);
        var itemIds = new List<string>();

        var reader = await command.ExecuteReaderAsync();

        if (reader.HasRows)
        {
            while (await reader.ReadAsync())
                itemIds.Add(reader["ItemId"].ToString());
        }

        return itemIds;
    }
}