# EventDrive

## Framework
 .NET 5
<br/>

## Description
The first endpoint of the web API will add our items to a Redis stream.  
The second endpoint will send a synchronization message through RabbitMQ, which will be consumed by the worker.  
The worker flow for handling the message is built with [System.Threading.Tasks.Dataflow](https://docs.microsoft.com/en-us/dotnet/standard/parallel-programming/dataflow-task-parallel-library), which allows to split the process into multiple steps that can be paralelized individually.  
The first step is to read the items from the Redis Stream (ReadStreamBlock.cs). A Consumer group is initialized which is configured to read all new messages for the stream.
After the messages were added to a collection we acknowledge them so that they are not processed again.  
The second step is the PersistenceBlock.cs. This block performs a bulk insert in the database for the items from the collection. If there is a high production rate of items
and the order of Insert does not matter, this step can be parallelised through its ExecutionDataflowBlockOptions.MaxDegreeOfParallelism.  
Additionally, the sum of all ExecutionDataflowBlockOptions.BoundedCapacity values shows how many messages can be stored in the Dataflow queue. When the limit is reached, TPL dataflow will pause the acceptance of new messages so that it does not get overwhelmed and crash the service. 

## Getting Started
The project is developed under Docker environment. In order to run the application we need to have [Docker for Desktop](https://www.docker.com/products/docker-desktop) installed and also a Hyper-V service enabled.

### Prerequisites
Since the API is configured to use HTTPS we must make sure we have dev certs configured.
https://tomssl.com/how-to-run-asp-net-core-3-1-over-https-in-docker-using-linux-containers/


### Starting the local environment
From the startup projcts drop down in Visual Studio choose ```docker-compose```  
At http://localhost:5002/health we can check the health of the worker application (useful for Kubernetes liveness/readiness probes)  
At https://localhost:5001/health we can check the heealth of the Web API  
At https://localhost:5001/swagger/index.html is the swagger of the API  

## Testing the application

### Manual Tests
In order to manually test the application navigate to the swagger url.  
There, each end-point will provide and example request (thanks to [Swashbuckle.AspNetCore.Filters](https://github.com/mattfrear/Swashbuckle.AspNetCore.Filters)) that can be executed.  
If wrong input is send the [FluentValidation](https://docs.fluentvalidation.net/en/latest/aspnet.html) in the API will cause it to return 404 status code and an error object.  
After sending some correct items to the API and synchronizing it with the worker service, we can check the database through SQL Server Management Studio.  
Use 'localhost' for server and user 'sa' with the password from the docker-compose.yml. The password is the value for the SA_PASSWORD key.  

### Automation Tests
The tests are built with the [Specflow](https://specflow.org/) testing framework. The main benefit of specflow is that the test steps are represented as human readable sentences and 
the test results can be exported. This allows us to show the test steps and results to a business analyser or a product owner and they will understand them.
Whatmore, they can potentially even provide the test steps and its up to the developers to only implement them.  

In order to run the tests follow the bellow steps:  
1. Run the application with ```Docker Compose``` option in Visual Studio
2. Wait until the browser opens a new window so that we know the application is running
3. From Visual Studio go to Debug menu and choose ```Detach All``` so that we stop the debugger but keep the containers up and running
4. Go to Test menu and choose ```Test Explorer```
5. Select the EventDrive.IntegrationTests dropdown and click Run

![image](https://user-images.githubusercontent.com/51854143/131394548-b2413eac-3176-40ad-a453-0cbe55bf52e9.png)

