using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Mediator.SourceGenerators.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Mediator.SourceGenerators;

[Generator]
public class BehaviourIncrementalGenerator : IIncrementalGenerator
{
    private record Behaviour(string Namespace, string FullyQualifiedClassName, Handler[] Handlers)
    {
        public string Namespace { get; } = Namespace;
        public string FullyQualifiedClassName { get; } = FullyQualifiedClassName;
        public Handler[] Handlers { get; } = Handlers;
    }

    private record Handler(string Type, string Request, string? Response)
    {
        public string Type { get; } = Type;
        public string Request { get; } = Request;
        public string? Response { get; } = Response;
    }
    
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var provider = context.SyntaxProvider.CreateSyntaxProvider(
                static (syntax, _) => syntax is ClassDeclarationSyntax,
                static (ctx, _) => Transform(ctx))
            .Where(x => x.Found)
            .Select((x, _) => x.Behaviour);
        
        context.RegisterSourceOutput(context.CompilationProvider.Combine(provider.Collect()),
            (ctx, t) => GenerateCode(ctx, t.Left, t.Right));
    }

    
    
    private static (Behaviour Behaviour, bool Found) Transform(GeneratorSyntaxContext context)
    {
        var declaration = (ClassDeclarationSyntax)context.Node;
        var symbol = (INamedTypeSymbol)context.SemanticModel.GetDeclaredSymbol(declaration)!;
        
        var result = GetBehaviourFromSymbol(symbol, context.SemanticModel.Compilation.GlobalNamespace, out var behaviour);
        return (behaviour!, result);
    }
    
    private static bool GetBehaviourFromSymbol(INamedTypeSymbol symbol, INamespaceSymbol globalSpace, out Behaviour? behaviour)
    {
        behaviour = null;
        bool symbolIsBehaviour = false;
        Handler[] handlers = [];
        foreach (var @interface in symbol.Interfaces)
        {
            var fullyQualifiedInterfaceName = @interface.ToString();
            if (fullyQualifiedInterfaceName.StartsWith("Mediator.Interfaces.IPipelineBehaviour") is false) continue;
            
            var arguments = @interface.TypeArguments;
            var handlerSymbol = (INamedTypeSymbol)arguments.First();
            handlers = FindHandlers(globalSpace, handlerSymbol);
            symbolIsBehaviour = true;
            break;
        }

        if (symbolIsBehaviour is false) return false;
        
        var @namespace = symbol.ContainingNamespace.ToString();
        var fullyQualifiedBehaviourName = symbol.ToString();
        fullyQualifiedBehaviourName = fullyQualifiedBehaviourName.Substring(0, fullyQualifiedBehaviourName.IndexOf('<'));
        behaviour = new Behaviour(@namespace, fullyQualifiedBehaviourName, handlers);
        return true;
    }


    private static bool IsDerivedFrom(INamedTypeSymbol symbol, INamedTypeSymbol handler)
    {
        if (symbol.IsUnboundGenericType) return false;
        if (symbol.IsGenericType is false) return false;
        if (SymbolEqualityComparer.Default.Equals(symbol.ConstructUnboundGenericType(), handler)) return true;
        foreach (var @interface in symbol.Interfaces)
        {
            var result = IsDerivedFrom(@interface, handler);
            if (result) return true;
        }

        return false;
    }
    
    private static Handler[] FindHandlers(INamespaceSymbol ns, INamedTypeSymbol handler)
    {
        List<Handler> handlers = [];
        foreach (var type in TypeHelper.GetAllTypes(ns))
        {
            if (type.IsGenericType) continue;
            
            foreach (var @interface in type.Interfaces)
            {
                if (IsDerivedFrom(@interface, handler.ConstructUnboundGenericType()) is false) continue;
                
                var input = @interface.ToString();
                string handlerType = input.Substring(0, input.IndexOf('<'));
                
                var arguments = @interface.TypeArguments;


                switch (arguments.Length)
                {
                    case 1:
                        handlers.Add(new Handler(handlerType, arguments[0].ToString(), null));
                        break;
                    case > 1:
                        handlers.Add(new Handler(handlerType,arguments[0].ToString(), arguments[1].ToString()));
                        break;
                    default:
                        continue;
                }
            }
        }

        return handlers.ToArray();
    }

    private static void GenerateCode(SourceProductionContext context, Compilation compilation,
        ImmutableArray<Behaviour> behaviours)
    {
        foreach (var assembly in compilation.SourceModule.ReferencedAssemblySymbols)
        {
            foreach (var type in TypeHelper.GetAllTypes(assembly.GlobalNamespace))
            {
                if (type.TypeKind != TypeKind.Class) continue;
                
                var result = GetBehaviourFromSymbol(type, compilation.GlobalNamespace, out var behaviour);
                if (result) behaviours = behaviours.Add(behaviour!);
            }
        }

        behaviours = behaviours.Where(x => x.Handlers.Length > 0).ToImmutableArray();
        var namespaces = behaviours.Select(x => x.Namespace).Distinct().ToArray();


        var entries = behaviours.SelectMany(behaviour =>
            behaviour.Handlers.Select(rr =>
                new
                {
                    behaviour.FullyQualifiedClassName,
                    HandlerType = rr.Type,
                    hasReturn = rr.Response is not null, 
                    Generic = rr.Response is null ? rr.Request : $"{rr.Request}, {rr.Response}",
                    rr.Request,
                    rr.Response
                })).ToArray();


        entries = entries.Distinct().ToArray();
        
        var dependencyInjectionBehaviours = entries.OrderBy(x => x.Request).ThenBy(x => x.HandlerType).Select(behaviour =>
            $"builder.Services.AddTransient<IBehaviourHandler<{behaviour.Generic}>, {behaviour.FullyQualifiedClassName}<{behaviour.Generic}>>();").Distinct().ToArray();
        
        var dependencyInjectionEnumerators =
            entries
                .Select(x => new {x.Generic}).Distinct().Select(input => $"builder.Services.AddTransient<Mediator.Interfaces.IBehaviourEnumerator<{input.Generic}>, Mediator.Implementations.BehaviourEnumerator<{input.Generic}>>();");
            
            
        
        context.AddSource("MediatorBehaviourDependencyInjectionExtensions.g.cs",
            $$"""
              // <auto-generated/>
              
              using System;
              using Mediator.Interfaces;
              using Mediator.Helpers;
              using Microsoft.Extensions.DependencyInjection;
              using Microsoft.Extensions.DependencyInjection.Extensions;
              {{string.Join("\n", namespaces.Select(@namespace => $"using {@namespace};"))}}
              
              namespace Mediator.DependencyInjection;
              
              internal static class BehaviourDependencyInjectionRegistrationExtensions
              {
                internal static MediatorBuilder AddPipelines(this MediatorBuilder builder)
                {
                    builder.UsePipelines();
                
                    // Registering Behaviours to dependency injection
                    {{string.Join("\n\n\t\t", dependencyInjectionBehaviours)}}
                    
                    // Registering Enumerators to dependency injection
                    {{string.Join("\n\n\t\t", dependencyInjectionEnumerators)}}
                    
                    return builder;
                }
              }
              """);
    }
}