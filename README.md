# EventDrive

## Framework
 .NET 5
<br/>

## Getting Started
The project is developed under Docker environment. In order to run the application we need to have Docker for Desktop installed and also a Hyper-V service enabled.

### Prerequisites
Since the API is configured to use HTTPS we must make sure we have dev certs configured.
https://tomssl.com/how-to-run-asp-net-core-3-1-over-https-in-docker-using-linux-containers/


### Starting the local environment
From the startup projcts drop down in Visual Studio choose ```docker-compose```  
At http://localhost:5002/health we can check the health of the worker application (useful for Kubernetes liveness/readiness probes)  
At https://localhost:5001/health we can check the heealth of the Web API  
At https://localhost:5001/swagger/index.html is the swagger of the API  

## Testing the application

The tests are built with the Specflow testing framework.  
In order to run the tests follow the bellow steps:  
1. Run the application with ```Docker Compose``` option in Visual Studio
2. Wait until the browser opens a new window so that we know the application is running
3. From Visual Studio go to Debug menu and choose ```Detach All``` so that we stop the debugger but keep the containers up and running
4. Go to Test menu and choose ```Test Explorer```
5. Run the test
