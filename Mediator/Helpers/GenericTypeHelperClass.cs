namespace Mediator.Helpers;

internal static class GenericTypeHelperClass
{
    
    /// <summary>
    /// Retrieve a list of type from the derived to the base type
    /// </summary>
    /// <param name="derived">The derived type</param>
    /// <param name="baseType">The base type</param>
    /// <returns>A list of types between derived and base type (both included)</returns>
    /// <exception cref="InvalidOperationException">Throws exception if "derived" parameter is not derived from "baseType" parameter</exception>
    internal static List<Type> GetInheritanceSteps(this Type derived, Type baseType)
    {
        var result = TraverseInheritance(derived, baseType, out var derivedTypes);
        return result 
            ? derivedTypes 
            : throw new InvalidOperationException($"{derived} type doesn't derive from {baseType}");
    }
    
    private static bool TraverseInheritance(Type derived, Type baseType, out List<Type> derivedTypes)
    {
        derivedTypes = [derived];
        if (derived.GetGenericTypeDefinition() == baseType.GetGenericTypeDefinition())
        {
            return true;
        }
        

        var interfaces = derived.GetInterfaces();
        if (interfaces.Length == 0) return false;
        foreach (var @interface in interfaces)
        {
            var result = TraverseInheritance(@interface, baseType, out var extended);
            if (result)
            {
                derivedTypes.AddRange(extended);
                return true;
            }
        }


        return false;
    }
    
    
    
    
    internal static bool IsDerivedFrom(this Type derived, Type baseType)
    {
        return TraverseInheritance(derived, baseType);
    }
    
    private static bool TraverseInheritance(Type derived, Type baseType)
    {
        if (derived.GetGenericTypeDefinition() == baseType.GetGenericTypeDefinition())
            return true;
        

        var interfaces = derived.GetInterfaces();
        if (interfaces.Length == 0) return false;
        foreach (var @interface in interfaces)
        {
            var result = TraverseInheritance(@interface, baseType);
            if (result) return true;
        }


        return false;
    }

    
}