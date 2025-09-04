namespace Mediator.Helpers;

public static class GenericTypeHelperClass
{
    public static bool IsDerivedFromWithSteps(this Type derived, Type baseType, out List<Type> derivedTypes)
    {
        return TraverseInheritance(derived, baseType, out derivedTypes);
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

    public static bool IsDerivedFrom(this Type derived, Type baseType)
    {
        return TraverseInheritance(derived, baseType); 
    }
}