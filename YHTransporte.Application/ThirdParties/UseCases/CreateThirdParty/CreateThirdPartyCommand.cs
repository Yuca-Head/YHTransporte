namespace YHTransporte.Application.ThirdParties.UseCases.CreateThirdParty;

public sealed record CreateThirdPartyCommand(string Name, bool IsCustomer = false, bool IsSupplier = false);