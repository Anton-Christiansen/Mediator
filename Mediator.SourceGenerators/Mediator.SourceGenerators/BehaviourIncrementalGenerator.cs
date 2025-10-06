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
    private record Information(string Namespace, string FullyQualifiedClassName, string HandlerType, RequestResponse[] RequestResponses)
    {
        public string Namespace { get; } = Namespace;
        public string FullyQualifiedClassName { get; } = FullyQualifiedClassName;
        public string HandlerType { get; } = HandlerType;
        public RequestResponse[] RequestResponses { get; } = RequestResponses;
    }

    private record RequestResponse(string Request, string? Response)
    {
        public string Request { get; } = Request;
        public string? Response { get; } = Response;
    }
    
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var provider = context.SyntaxProvider.CreateSyntaxProvider(
                static (syntax, _) => syntax is ClassDeclarationSyntax,
                static (ctx, _) => Transform(ctx))
            .Where(x => x.Found)
            .Select((x, _) => x.Info);
        
        context.RegisterSourceOutput(context.CompilationProvider.Combine(provider.Collect()),
            (ctx, t) => GenerateCode(ctx, t.Left, t.Right));
    }

    
    
    private static (Information Info, bool Found) Transform(GeneratorSyntaxContext context)
    {
        var declaration = (ClassDeclarationSyntax)context.Node;
        var symbol = (INamedTypeSymbol)context.SemanticModel.GetDeclaredSymbol(declaration)!;
        
        var result = GetInformationFromSymbol(symbol, context.SemanticModel.Compilation.GlobalNamespace, out var information);
        return (information!, result);
    }
    
    private static bool GetInformationFromSymbol(INamedTypeSymbol symbol, INamespaceSymbol globalSpace, out Information? information)
    {
        information = null;
        string handlerName = "";
        bool found = false;
        RequestResponse[] requestResponses = [];
        foreach (var @interface in symbol.Interfaces)
        {
            if (@interface.Name.Contains("IPipelineBehaviour") is false) continue;
            var arguments = @interface.TypeArguments;
            var handlerSymbol =  (INamedTypeSymbol)arguments.First();
            requestResponses = FindRequestResponses(globalSpace, handlerSymbol);

            var interfaceName = @interface.ToString();
            var start = interfaceName.IndexOf('<');
            var end = interfaceName.IndexOf('<', start + 1);
            handlerName = interfaceName.Substring(start + 1, end - start - 1);
            found = true;
            break;
        }

        if (found is false) return false;
        
        var @namespace = symbol.ContainingNamespace.ToString();
        var className = symbol.Name;
        information = new Information(@namespace, className, handlerName, requestResponses);
        return true;
    }

    
    
    private static RequestResponse[] FindRequestResponses(INamespaceSymbol ns, INamedTypeSymbol handler)
    {
        List<RequestResponse> requestResponses = [];
        foreach (var type in TypeHelper.GetAllTypes(ns))
        {
            if (type.IsGenericType) continue;
            
            foreach (var @interface in type.AllInterfaces)
            {
                if (@interface.IsUnboundGenericType) continue;
                if (@interface.IsGenericType is false) continue;
                
                var unbound = @interface.ConstructUnboundGenericType();
                var unboundHandler = handler.ConstructUnboundGenericType();
                
                if (SymbolEqualityComparer.Default.Equals(unbound, unboundHandler) is false) continue;

                var arguments = @interface.TypeArguments;


                if (arguments.Length == 1)
                {
                    var rr = new RequestResponse(arguments[0].ToString(), null);
                    requestResponses.Add(rr);
                }
                else if (arguments.Length > 1)
                {
                    var rr = new RequestResponse(arguments[0].ToString(), arguments[1].ToString());
                    requestResponses.Add(rr);
                }
                else
                {
                    throw new NotImplementedException();
                }
                
            }
        }

        return requestResponses.ToArray();
    }

    private static void GenerateCode(SourceProductionContext context, Compilation compilation,
        ImmutableArray<Information> information)
    {
        foreach (var assembly in compilation.SourceModule.ReferencedAssemblySymbols)
        {
            foreach (var type in TypeHelper.GetAllTypes(assembly.GlobalNamespace))
            {
                if (type.TypeKind != TypeKind.Class) continue;
                
                var result = GetInformationFromSymbol(type, compilation.GlobalNamespace, out var info);
                if (result) information = information.Add(info!);
            }
        }

        information = information.Where(x => x.RequestResponses.Length > 0).ToImmutableArray();
        
        
        var namespaces = information.Select(x => x.Namespace).Distinct().ToArray();


        var test = information.SelectMany(info =>
            info.RequestResponses.Select(rr =>
                new
                {
                    info.FullyQualifiedClassName,
                    info.HandlerType,
                    Generic = rr.Response is null ? rr.Request : $"{rr.Request}, {rr.Response}"
                })).ToArray();


        test = test.Distinct().ToArray();
        
        var lines = test.Select(info =>
            $"builder.Services.AddTransient<IPipelineBehaviour<{info.HandlerType}<{info.Generic}>, {info.Generic}>, {info.FullyQualifiedClassName}<{info.Generic}>>();").ToArray();
            
            
        
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
                    {{string.Join("\n\n\t\t", lines)}}
                    
                    return builder;
                }
              }
              """);
    }
}