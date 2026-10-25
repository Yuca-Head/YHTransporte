using Moq;
using YHTransporte.Application.Addresses.Repositories;
using YHTransporte.Application.Orders.Repositories;
using YHTransporte.Application.Orders.UseCases.CreateOrder;
using YHTransporte.Application.ThirdParties.Repositories;
using YHTransporte.Core.Entities;

namespace YHTransporte.Testing.AppsTests;

public class OrderUseCreationMockTests
{
    private readonly Mock<IOrderRepository> _orderRepo = new();
    private readonly Mock<IAddressRepository> _addressRepo = new();
    private readonly Mock<IThirdPartyRepository> _thirdPartyRepo = new();

    private CreateOrderHandler CreateSut() =>
        new(
            _orderRepo.Object,
            _addressRepo.Object,
            _thirdPartyRepo.Object);

    private static CreateOrderCommand CreateCommand() =>
        new(
            SupplierId: 20,
            CustomerId: 10,
            OriginId: 1,
            DestinationId: 2);

    private void SetupAddresses(bool originExists = true,
                                bool destinationExists = true)
    {
        _addressRepo
            .Setup(x => x.Exists(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(originExists);

        _addressRepo
            .Setup(x => x.Exists(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(destinationExists);
        
        int[] key1 = [1];
        int[] key2 = [2];
        _addressRepo
            .Setup(x => x.GetByKeysAsync(key1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((IEnumerable<int> x, CancellationToken y) => [new Address("-", new("-", new("-"))) {Key = 1}]);
        _addressRepo
            .Setup(x => x.GetByKeysAsync(key2, It.IsAny<CancellationToken>()))
            .ReturnsAsync((IEnumerable<int> x, CancellationToken y) => [new Address("-", new("-", new("-"))) {Key = 2}]);
    }

    private async Task<ThirdParty> CreateCustomer()
    {
        var party = new ThirdParty("Customer");
        party.AddAddresses(await _addressRepo.Object.GetByKeysAsync([2], default));
        party.BecomeCustomer();
        return party;
    }

    private async Task<ThirdParty> CreateSupplierAsync()
    {
        var party = new ThirdParty("Supplier");
        party.AddAddresses(await _addressRepo.Object.GetByKeysAsync([1], default));
        party.BecomeSupplier();
        return party;
    }

    private void SetupParty(int id, ThirdParty party)
    {
        _thirdPartyRepo
            .Setup(x => x.GetByKeysAsync(
                It.Is<IEnumerable<int>>(
                    keys => keys.Contains(id)),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([party]);
    }

    private void SetupMissingParty(int id)
    {
        _thirdPartyRepo
            .Setup(x => x.GetByKeysAsync(
                It.Is<IEnumerable<int>>(
                    keys => keys.Contains(id)),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<ThirdParty>());
    }

    [Fact]
    public async Task Handle_OriginDoesNotExist_ReturnsAddressNotFound()
    {
        SetupAddresses(originExists: false);
        var command = CreateCommand();

        var result = await CreateSut().Handle(command);

        Assert.True(result.IsT1);
        Assert.Equal(
            1,
            result.AsT1.Ids.First()/* AddressNotFound */);

        _addressRepo.Verify(
            x => x.Exists(2, It.IsAny<CancellationToken>()),
            Times.Never);

        _thirdPartyRepo.Verify(
            x => x.GetByKeysAsync(
                It.IsAny<IEnumerable<int>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _orderRepo.Verify(
            x => x.AddAsync(
                It.IsAny<Order>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_DestinationDoesNotExist_ReturnsAddressNotFound()
    {
        SetupAddresses(destinationExists: false);

        var result = await CreateSut().Handle(CreateCommand());

        Assert.True(result.IsT1);

        _thirdPartyRepo.Verify(
            x => x.GetByKeysAsync(
                It.IsAny<IEnumerable<int>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _orderRepo.Verify(
            x => x.AddAsync(
                It.IsAny<Order>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_CustomerDoesNotExist_ReturnsCustomerNotFound()
    {
        SetupAddresses();
        SetupMissingParty(10);
        SetupParty(20, await CreateSupplierAsync());

        var result = await CreateSut().Handle(CreateCommand());

        Assert.True(result.IsT2);

        _orderRepo.Verify(
            x => x.AddAsync(
                It.IsAny<Order>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_PartyHasNoCustomerRole_ReturnsCustomerNotFound()
    {
        SetupAddresses();
        SetupParty(10, new ThirdParty("Without customer role"));
        SetupParty(20, await CreateSupplierAsync());

        var result = await CreateSut().Handle(CreateCommand());

        Assert.True(result.IsT2);

        _orderRepo.Verify(
            x => x.AddAsync(
                It.IsAny<Order>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_SupplierDoesNotExist_ReturnsSupplierNotFound()
    {
        SetupAddresses();
        SetupParty(10, await CreateCustomer());
        SetupMissingParty(20);

        var result = await CreateSut().Handle(CreateCommand());

        Assert.True(result.IsT3);

        _orderRepo.Verify(
            x => x.AddAsync(
                It.IsAny<Order>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_PartyHasNoSupplierRole_ReturnsSupplierNotFound()
    {
        SetupAddresses();
        SetupParty(10, await CreateCustomer());
        SetupParty(20, new ThirdParty("Without supplier role"));

        var result = await CreateSut().Handle(CreateCommand());

        Assert.True(result.IsT3);

        _orderRepo.Verify(
            x => x.AddAsync(
                It.IsAny<Order>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ValidData_CreatesOrderAndReturnsSuccess()
    {
        SetupAddresses();
        SetupParty(10, await CreateCustomer());
        SetupParty(20, await CreateSupplierAsync());

        _orderRepo
            .Setup(x => x.AddAsync(
                It.IsAny<Order>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await CreateSut().Handle(CreateCommand());

        Assert.True(result.IsT0);

        _orderRepo.Verify(
            x => x.AddAsync(
                It.Is<Order>(order =>
                    order.Supplier != null &&
                    order.Customer != null),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}