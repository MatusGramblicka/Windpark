FROM mcr.microsoft.com/dotnet/core/aspnet:3.1

COPY bin/Release/netcoreapp3.1/publish/ App/
WORKDIR /App 

ENV ASPNETCORE_ENVIRONMENT Production
EXPOSE 3001 
   
ENTRYPOINT ["dotnet", "WindparkAPIConsumer.dll", "--urls", "http://*:3001"] 
