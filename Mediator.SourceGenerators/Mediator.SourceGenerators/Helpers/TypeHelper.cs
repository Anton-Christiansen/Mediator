using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace Mediator.SourceGenerators.Helpers;

public static class TypeHelper
{
    public static IEnumerable<INamedTypeSymbol> GetAllTypes(INamespaceSymbol ns)
    {
        foreach (var type in ns.GetTypeMembers())
            yield return type;

        foreach (var nestedNs in ns.GetNamespaceMembers())
        {
            foreach (var t in GetAllTypes(nestedNs))
            {
                yield return t;
                
                foreach (var l in t.GetTypeMembers())
                {
                    yield return l;
                }
            }
        }
    }
}