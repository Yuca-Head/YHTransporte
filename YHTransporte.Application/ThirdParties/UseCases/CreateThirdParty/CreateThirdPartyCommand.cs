namespace YHTransporte.Application.ThirdParties.UseCases.CreateThirdParty;

public sealed record CreateThirdPartyCommand
{
    public CreateThirdPartyCommand(string name = "", bool isCustomer = false, bool isSupplier = false)
    {
        Name = name;
        IsCustomer = isCustomer;
        IsSupplier = isSupplier;   
    }

    public bool IsCustomer {get; init;}
    public bool IsSupplier {get; init;}

    public string Name {get; init => field = value.Trim();}
}