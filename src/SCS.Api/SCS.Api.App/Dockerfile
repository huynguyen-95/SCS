# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["SCS.Api.App/SCS.Api.App.csproj", "SCS.Api.App/"]
COPY ["SCS.Api.Domain/SCS.Api.Domain.csproj", "SCS.Api.Domain/"]
RUN dotnet restore "SCS.Api.App/SCS.Api.App.csproj"
COPY . .
WORKDIR "/src/SCS.Api.App"
RUN dotnet publish "SCS.Api.App.csproj" -c Debug -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081
COPY --from=build /app/publish .
COPY SCS.Api.App/appsettings.json .
COPY SCS.Api.App/appsettings.Development.json .

# Environment configuration
ENV ASPNETCORE_ENVIRONMENT=Development

# Override appsettings.json placeholder values
ENV AWS__Region=<AWS_REGION>
ENV AWS__AccessKey=<YOUR_ACCESS_KEY>
ENV AWS__SecretKey=<YOUR_SECRET_KEY>
ENV AWS__QueueUrl=<QUEUE_URL>
ENV AWS__BucketName=<YOUR_BUCKET_NAME>
ENV ConnectionStrings__DefaultConnection=<YOUR_CONNECTION_STRING>

ENTRYPOINT ["dotnet", "SCS.Api.App.dll"]