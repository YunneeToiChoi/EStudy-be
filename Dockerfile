#################### BUILD ENVIROMENT #####################
# Use the official .NET SDK image as the build environment
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env 
WORKDIR /app

# Copy the .csproj file and restore the dependencies (via NuGet)
COPY *.csproj ./ 
RUN dotnet restore

# Copy the rest of the application source code
COPY . ./

# Publish the application in Release mode and output to the "out" folder
RUN dotnet publish -c Release -o out
#################### RUNTIME ENVIROMENT #####################

# Use a smaller image to run the application (ASP.NET runtime image)
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
    
# Copy the published application from the build environment
COPY --from=build-env /app/out .

# Expose port 80 to allow communication with the container
EXPOSE 80

# Set the entry point for the container to run the application
ENTRYPOINT ["dotnet", "study4-be.dll"]
