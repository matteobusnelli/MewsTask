# Mews backend developer task

## Running the .NET Exchange Rate Updater

### Prerequisites
- .NET 10.0 SDK

### Build and Run the Application

```bash
# Build the solution
dotnet build Task/ExchangeRateUpdater.sln

# Run the application
dotnet run --project Task/ExchangeRateUpdater.csproj
```

The application will fetch current exchange rates from the Czech National Bank and display them in the console.

### Run Unit Tests

```bash
# Run all tests
dotnet test Task.Tests/ExchangeRateUpdater.Tests.csproj

# Run tests with detailed output
dotnet test Task.Tests/ExchangeRateUpdater.Tests.csproj --verbosity normal
```