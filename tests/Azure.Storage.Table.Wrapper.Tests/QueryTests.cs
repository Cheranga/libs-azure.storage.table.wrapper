﻿using Azure.Data.Tables;
using FluentAssertions;
using Microsoft.Extensions.Azure;
using Moq;

namespace Azure.Storage.Table.Wrapper.Tests;

public static class QueryTests
{
    [Fact(DisplayName = "Get an existing entity from table")]
    public static async Task GetExistingEntity()
    {
        var tableClient = new Mock<TableClient>();
        tableClient
            .Setup(
                x =>
                    x.GetEntityAsync<ProductDataModel>(
                        "TECH",
                        "PROD1",
                        It.IsAny<IEnumerable<string>>(),
                        It.IsAny<CancellationToken>()
                    )
            )
            .ReturnsAsync(
                TestResponse<ProductDataModel>.Success(ProductDataModel.New("TECH", "PROD1", 100))
            );

        var tableServiceClient = new Mock<TableServiceClient>();
        tableServiceClient.Setup(x => x.GetTableClient("products")).Returns(tableClient.Object);

        var factory = new Mock<IAzureClientFactory<TableServiceClient>>();
        factory.Setup(x => x.CreateClient("test")).Returns(tableServiceClient.Object);
        var tableService = new TableService(factory.Object);
        var op = await tableService.GetEntityAsync<ProductDataModel>(
            "test",
            "products",
            "TECH",
            "PROD1",
            new CancellationToken()
        );

        var succOp = op as TableOperation.QuerySingleOperation<ProductDataModel>;
        succOp.Should().NotBeNull();
        succOp!.Entity.Category.Should().Be("TECH");
        succOp!.Entity.Id.Should().Be("PROD1");
        succOp!.Entity.Price.Should().Be(100);
    }

    [Fact(DisplayName = "Entity does not exist")]
    public static async Task EntityDoesNotExist()
    {
        var tableClient = new Mock<TableClient>();
        tableClient
            .Setup(
                x =>
                    x.GetEntityAsync<ProductDataModel>(
                        "TECH",
                        "PROD1",
                        It.IsAny<IEnumerable<string>>(),
                        It.IsAny<CancellationToken>()
                    )
            )
            .ReturnsAsync(TestResponse<ProductDataModel>.Fail("entity not found"));

        var tableServiceClient = new Mock<TableServiceClient>();
        tableServiceClient.Setup(x => x.GetTableClient("products")).Returns(tableClient.Object);

        var factory = new Mock<IAzureClientFactory<TableServiceClient>>();
        factory.Setup(x => x.CreateClient("test")).Returns(tableServiceClient.Object);
        var tableService = new TableService(factory.Object);
        var op = await tableService.GetEntityAsync<ProductDataModel>(
            "test",
            "products",
            "TECH",
            "PROD1",
            new CancellationToken()
        );

        var failedOp = op as TableOperation.FailedOperation;
        failedOp.Should().NotBeNull();
        failedOp.Error.Code.Should().Be(ErrorCodes.EntityDoesNotExist);
    }

    [Fact(DisplayName = "Filter returns entities")]
    public static async Task FilterReturnsEntities()
    {
        var tableClient = new Mock<TableClient>();
        tableClient
            .Setup(
                x =>
                    x.QueryAsync<ProductDataModel>(
                        x => x.Category == "TECH",
                        It.IsAny<int?>(),
                        It.IsAny<IEnumerable<string>>(),
                        It.IsAny<CancellationToken>()
                    )
            )
            .Returns(
                AsyncPageable<ProductDataModel>.FromPages(
                    new[]
                    {
                        Page<ProductDataModel>.FromValues(
                            new[]
                            {
                                ProductDataModel.New("TECH", "PROD1", 100),
                                ProductDataModel.New("TECH", "PROD2", 150)
                            },
                            It.IsAny<string>(),
                            It.IsAny<Response>()
                        )
                    }
                )
            );

        var tableServiceClient = new Mock<TableServiceClient>();
        tableServiceClient.Setup(x => x.GetTableClient("products")).Returns(tableClient.Object);

        var factory = new Mock<IAzureClientFactory<TableServiceClient>>();
        factory.Setup(x => x.CreateClient("test")).Returns(tableServiceClient.Object);
        var tableService = new TableService(factory.Object);
        var op = await tableService.GetEntityListAsync<ProductDataModel>(
            "test",
            "products",
            x => x.Category == "TECH",
            new CancellationToken()
        );

        var succOp = op as TableOperation.QueryListOperation<ProductDataModel>;
        succOp.Should().NotBeNull();
        succOp!.Entities.Count.Should().Be(2);
    }

    [Fact(DisplayName = "Filter does not return entities")]
    public static async Task FilterDoesNotReturnEntities()
    {
        var tableClient = new Mock<TableClient>();
        tableClient
            .Setup(
                x =>
                    x.QueryAsync<ProductDataModel>(
                        x => x.Category == "TECH",
                        It.IsAny<int?>(),
                        It.IsAny<IEnumerable<string>>(),
                        It.IsAny<CancellationToken>()
                    )
            )
            .Returns(
                AsyncPageable<ProductDataModel>.FromPages(
                    Enumerable.Empty<Page<ProductDataModel>>()
                )
            );

        var tableServiceClient = new Mock<TableServiceClient>();
        tableServiceClient.Setup(x => x.GetTableClient("products")).Returns(tableClient.Object);

        var factory = new Mock<IAzureClientFactory<TableServiceClient>>();
        factory.Setup(x => x.CreateClient("test")).Returns(tableServiceClient.Object);
        var tableService = new TableService(factory.Object);
        var op = await tableService.GetEntityListAsync<ProductDataModel>(
            "test",
            "products",
            x => x.Category == "TECH",
            new CancellationToken()
        );

        var failedOp = op as TableOperation.FailedOperation;
        failedOp.Should().NotBeNull();
        failedOp!.Error.Code.Should().Be(ErrorCodes.EntityListDoesNotExist);
    }
}