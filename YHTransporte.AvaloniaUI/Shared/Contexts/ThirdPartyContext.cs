using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using OneOf;
using OneOf.Types;
using YHTransporte.Application.Shared.Results;
using YHTransporte.Application.ThirdParties.Dtos;
using YHTransporte.Application.ThirdParties.Results;
using YHTransporte.Application.ThirdParties.UseCases.GetThirdParty;
using YHTransporte.AvaloniaUI.Shared.Messaging;

namespace YHTransporte.AvaloniaUI.Shared.Contexts;


public sealed class ThirdPartyContext : IInitiableContext
{
    private readonly GetThirdPartyHandler _queryHandler;
    private readonly Dictionary<int, ThirdPartyDetailsDto> _parties = [];

    public event Action? ThirdPartiesChanged;
    
    //public IReadOnlyCollection<ThirdPartyDetailsDto> Customers => [.. _parties.Values.Where(x => x.Customer is not null)];
    public IReadOnlyCollection<ThirdPartyDetailsDto> ThirdParties => [.. _parties.Values];
    public ThirdPartyContext(GetThirdPartyHandler getThirdPartyHandler)
    {
        _queryHandler = getThirdPartyHandler;


        WeakReferenceMessenger.Default.Register<ThirdPartyUpdateMessage>(this, async (_,m) =>
        {
            if(m.ChangeType is Enums.ContextChangeType.Creation)
            {
                await ReDoThirdParties();
                ThirdPartiesChanged?.Invoke();
                return;
            }
        
            await GenericContexts.UpdateDtoList(_parties, m, 
            async (x, token) => 
            {
                var result = await _queryHandler.GetThirdPartyDetails(x.Select(y => new GetThirdPartyQuery(y)), token);


                if(!result.IsT0)
                    throw new InvalidOperationException("Every ThirdParty key was expected to exist");
                return result.AsT0.Value;

            }, d => d.Key);

            ThirdPartiesChanged?.Invoke();
        });
    }   


    public async Task Init()
    {
        await ReDoThirdParties();
    }
    
    private async Task ReDoThirdParties()
    {
        _parties.Clear();

        var parties = await _queryHandler.LoadThirdParties();
        foreach(var party in parties)
            _parties.Add(party.Key, party);
        
    }

    public async Task<ThirdPartyDetailsDto?> GetById(int id) 
    {
        var result = await GenericContexts.TryGetAsync
        ([new GetThirdPartyQuery(id)], x => x.Key, _parties, p => p.Key, _queryHandler.GetThirdPartyDetails, [.. ThirdParties], default);
        

        if(result.IsT0 && result.AsT0.Value is null)
            return null;

        return result.AsT0.Value.First();
        
    }
}