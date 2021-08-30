namespace EventDrive.IntegrationTests.Steps
{
    using Common;
    using DTOs;
    using DTOs.Commands;
    using FluentAssertions;
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Threading.Tasks;
    using TechTalk.SpecFlow;

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
            _listOfDtos = Enumerable
            .Range(0, 3)
            .ToList()
            .Select(x =>
            {
                var guid = Guid.NewGuid().ToString();

                return new MyDTO
                {
                    Id = guid,
                    Name = guid + "lala"
                };
            }).ToList();
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
            // Arrange
            var expectedResult = _listOfDtos.Select(x => x.Id).ToList();
            var actualResult = new List<string>();

            // Act
            using var connection = new SqlConnection("Data Source=mssql;Initial Catalog=EventDriveDB;User ID=user;Password=simplePWD123!"); // move to appsettings

            await connection.OpenAsync();

            var command = new SqlCommand("SELECT * FROM dbo.Items", connection);

            var reader = await command.ExecuteReaderAsync();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    actualResult.Add(reader["ItemId"].ToString());
                }
            }

            foreach (var id in expectedResult)
            {
                actualResult.Contains(id).Should().BeTrue();
            }

            // Assert
            actualResult.Should().BeEquivalentTo(expectedResult);
        }
    }
}