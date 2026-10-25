namespace YHTransporte.Core.Entities;

public class CustomerRole
{
    public CustomerRole(int thirdPartyKey)
    {
        ThirdPartyKey = thirdPartyKey;
    }
    
    public int ThirdPartyKey {get; init;}
}