using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace YHTransporte.AvaloniaUI.Shared;

public static class ResultHandler
{
    public static bool TryParseToString(object? value, out IEnumerable<string> results)
    {
        List<string> list = [];
        
        if(value is not null)
            if (value is IEnumerable collection)
                foreach(var r in collection.OfType<string>())
                    list.Add(r);
            else if(value is string r)
                list.Add(r);
        
        results = list;
        
        return results.Any();
    }
}