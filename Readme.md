﻿<!-- markdownlint-disable MD033 MD041 -->
<div align="center">

<img src="data-table.png" alt="TypedSpark.NET" width="150px"/>

# Azure Data Table Wrapper

Wrapper functions to easily interact with Azure table storage and Cosmos

</div>

> The library uses Microsoft's `Azure.Data.Tables` package to perform data operations.</p>
> The library separates commands and queries, and provides separate methods to be used easily on your data tables.

## :tada: Features

:bulb: Separation of command and query operations.

:bulb: Query and command responses are strongly typed with clear intentions of what will be returned.

:bulb: Support for either managed identity or connection string through dependency injection.

## :tada: Usage

:high_brightness: Register the library with your Microsoft dependency injection

This will register the connectivity as a named instance. So you can register as much as named clients as you wish in your application.</br>
In the below example `ProductsDomain` is the named client.

```csharp

// If you are using an ASP.NET application
builder.Services.RegisterTablesWithConnectionString(
            "ProductsDomain",
            "[Connection string for the storage account]"
        );

// This is from the sample console application in the examples section.
var host = Host.CreateDefaultBuilder()
    .ConfigureServices(services =>
    {
        services.RegisterTablesWithConnectionString(
         "ProductsDomain",
         "[Connection string for the storage account]"
        );
    })
    .Build();
    
```

:high_brightness: Inject the dependencies `IQueryService` or `ICommandService` or both to your class.

:high_brightness: Use the respective methods to interact with the table.

### Queries

```csharp

// queries
var readOp = await queryService.GetEntityAsync<ProductDataModel>(
        "ProductsDomain", // the named client
        "products", // the table name
        "TECH", // partition key
        "PROD1", // row key
        new CancellationToken()
    );

// depending on the response, you can decide what to do next
readOp.Response switch
      {
          QueryResult.SingleResult<ProductDataModel> r
              => GetSuccessResponse(r),
          QueryResult.QueryFailedResult f => GetFailedResponse(f),
          _ => GetServerErrorResponse()
      }

```

### Commands

```csharp

// commands
var productDataModel = ProductDataModel.New("TECH", "PROD1", 259.99d);
var commandOp = await commandService.UpsertAsync(
    "ProductsDomain", // named client
    "products", // table name
    productDataModel, // table entity to upsert
    new CancellationToken()
);

// depending on the response, you can decide what to do next
commandOp.Operation switch
  {
      CommandOperation.CommandSuccessOperation _ => // do stuff,
      CommandOperation.CommandFailedOperation f
          => // do error stuff,
      _ => throw new NotSupportedException()
  }

```

:high_brightness: and, that's it!

## Attributions

[Icons created by juicy_fish](https://www.flaticon.com/free-icon/data-table_3575798)


