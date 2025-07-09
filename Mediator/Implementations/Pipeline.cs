using Mediator.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Mediator.Implementations;

internal class CommandPipeline<THandler, TRequest>(THandler handler, List<IPipelineBehaviour<TRequest>> behaviours)
    where THandler : IRequestHandler<TRequest>
{
    private IPipelineBehaviour<TRequest>[] Behaviours { get; } = behaviours.ToArray();
    private THandler Handler { get; } = handler;
    private CancellationToken CancellationToken { get; set; }
    
    public async Task ExecuteAsync(TRequest request, CancellationToken cancellationToken)
    {
        CancellationToken = cancellationToken;
        
        
        switch (Behaviours.Length)
        {
            case 0:
                await FinalExecutionAsync(request);
                return;
            case 1:
                await Behaviours[0].ExecuteAsync(request, FinalExecutionAsync, cancellationToken);
                return;
            default:
                await Behaviours[0].ExecuteAsync(request, IterateStepsAsync, cancellationToken);
                break;
        }
    }

    private async Task FinalExecutionAsync(TRequest request)
    {
        await Handler.HandleAsync(request, CancellationToken);
    }


    private int _index = 1;
    private async Task IterateStepsAsync(TRequest request)
    {
        if (_index < Behaviours.Length - 1)
        {
            await Behaviours[_index].ExecuteAsync(request, IterateStepsAsync, CancellationToken);
            _index++;
            return;
        }

        if (_index == Behaviours.Length - 1)
        {
            await Behaviours[_index].ExecuteAsync(request, FinalExecutionAsync, CancellationToken);
            _index++;
            return;
        }
        
        
        if (_index != Behaviours.Length) throw new IndexOutOfRangeException("No more steps");
    }
}


internal class QueryPipeline<THandler, TRequest, TResponse>(THandler handler, List<IPipelineBehaviour<TRequest, TResponse>> behaviours)
    where THandler : IRequestHandler<TRequest, TResponse>
{
    private IPipelineBehaviour<TRequest, TResponse>[] Behaviours { get; } = behaviours.ToArray();
    private THandler Handler { get; } = handler;
    
    private CancellationToken CancellationToken { get; set; }


    public async Task<TResponse> ExecuteAsync(TRequest request, CancellationToken cancellationToken)
    {
        CancellationToken = cancellationToken;
        
        switch (Behaviours.Length)
        {
            case 0:
                return await FinalExecutionAsync(request);
            case 1:
                return await Behaviours[0].ExecuteAsync(request, FinalExecutionAsync, cancellationToken); ;
            default:
                return await Behaviours[0].ExecuteAsync(request, IterateStepsAsync, cancellationToken);
        }
    }

    private async Task<TResponse> FinalExecutionAsync(TRequest request)
    {
        return await Handler.HandleAsync(request, CancellationToken);
    }


    private int _index = 1;

    private async Task<TResponse> IterateStepsAsync(TRequest request)
    {
        if (_index < Behaviours.Length - 1)
        {
            _index += 1;
            return await Behaviours[_index - 1].ExecuteAsync(request, IterateStepsAsync, CancellationToken);
        }

        if (_index == Behaviours.Length - 1)
        {
            _index += 1;
            return await Behaviours[_index - 1].ExecuteAsync(request, FinalExecutionAsync, CancellationToken);
        }

        throw new IndexOutOfRangeException("No more steps");
    }
}