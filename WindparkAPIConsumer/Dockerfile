FROM mcr.microsoft.com/dotnet/aspnet:6.0

COPY bin/Release/net6.0/publish/ App/
WORKDIR /App 

ENV ASPNETCORE_ENVIRONMENT Production
EXPOSE 3001 
   
ENTRYPOINT ["dotnet", "WindParkAPIConsumer.dll", "--urls", "http://*:3001"] 
