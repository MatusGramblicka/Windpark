Prerequisite

    Docker on your Dev Machine
    Dev Enviroment for your choosen Techstack
    Github to upload your code to share with us

Create an Application that runs in a docker Container, that calls the Windpark API frequently and aggregates the Data for Each Park and Turbine into a 5 min. 
Value for Windspeed and Current Production in Megawatt. 
The Aggregated Data should be send to an RabbitMQ Exchange with a Queue attached that receives the Data.
Optional: Create a Consumer that receives Data from the RabbitMQ Queue and displays it as an API

Hints

    Choose a tech-stack of your choice
    Everything should run in Docker Containers
    RabbitMQ can be hosted in Single Instance or Cluster
    timelimit 4 hours

Windpark API http://renewables-codechallenge.azurewebsites.net/swagger/index.html
