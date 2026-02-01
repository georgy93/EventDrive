namespace EventDrive.IntegrationTests.Steps;

using Common;
using DapperQueryBuilder;
using DTOs;
using DTOs.Commands;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Reqnroll;
using Xunit;

[Binding]
public class IntegrationStepDefinitions
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
        await _eventDriveAPI.AddItemsToRedisAsync(new AddItemsCommand(_listOfDtos), TestContext.Current.CancellationToken);
    }

    [When(@"a request for synchronization event is sent")]
    public async Task WhenARequestForSynchronizationEventIsSent()
    {
        await _eventDriveAPI.NotifyItemsAddedAsync(TestContext.Current.CancellationToken);
    }

    [Then(@"the items should be found in the data store")]
    public async Task ThenTheItemsShouldBeFoundInTheDataStore()
    {
        // wait some time for the worker to insert data.
        // Other option is to poll the database for the items or use SQLDependency for change notification
        await Task.Delay(TimeSpan.FromSeconds(3), TestContext.Current.CancellationToken);

        // Arrange
        var expectedResultIds = _listOfDtos.Select(x => x.Id).ToList();

        // Act
        var actualResultIds = await GetItemsFromDataStoreAsync(expectedResultIds, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equivalent(expectedResultIds, actualResultIds);
    }

    private async Task<IEnumerable<string>> GetItemsFromDataStoreAsync(IEnumerable<string> ids, CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(_dbConnectionString);
        await connection.OpenAsync(cancellationToken);

        var query = connection
            .QueryBuilder(@$"
                SELECT ItemId FROM dbo.Items
                WHERE ItemId IN {ids};");

        return await query.QueryAsync<string>(commandTimeout: 10, cancellationToken: cancellationToken);;
    }
}