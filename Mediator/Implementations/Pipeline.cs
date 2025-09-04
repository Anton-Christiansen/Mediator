using Mediator.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Mediator.Implementations;

internal class CommandPipeline<THandler, TRequest>(THandler handler, IEnumerable<IPipelineBehaviour<TRequest>> behaviours)
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
            default:
                await Behaviours[0].ExecuteAsync(request, IterateStepsAsync, cancellationToken);
                break;
        }
    }

    private async Task FinalExecutionAsync(TRequest request)
    {
        await Handler.HandleAsync(request, CancellationToken);
    }


    private int Index { get; set; } = 1;

    private async Task IterateStepsAsync(TRequest request)
    {
        if (Index < Behaviours.Length - 1)
        {
            Index += 1;
            await Behaviours[Index - 1].ExecuteAsync(request, IterateStepsAsync, CancellationToken);
            return;
        }

        // Last
        if (Index == Behaviours.Length - 1)
        {
            Index += 1;
            await Behaviours[Index - 1].ExecuteAsync(request, FinalExecutionAsync, CancellationToken);
            return;
        }

        throw new IndexOutOfRangeException("No more steps");
    }
}

internal class QueryPipeline<THandler, TRequest, TResponse>(
    THandler handler,
    IEnumerable<IPipelineBehaviour<TRequest, TResponse>> behaviours)
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
                return await Behaviours[0].ExecuteAsync(request, FinalExecutionAsync, cancellationToken);
                ;
            default:
                return await Behaviours[0].ExecuteAsync(request, IterateStepsAsync, cancellationToken);
        }
    }

    private async Task<TResponse> FinalExecutionAsync(TRequest request)
    {
        return await Handler.HandleAsync(request, CancellationToken);
    }


    private int Index { get; set; } = 1;

    private async Task<TResponse> IterateStepsAsync(TRequest request)
    {
        if (Index < Behaviours.Length - 1)
        {
            Index += 1;
            var result = await Behaviours[Index - 1].ExecuteAsync(request, IterateStepsAsync, CancellationToken);
            return result;
        }

        // Last step
        if (Index == Behaviours.Length - 1)
        {
            Index += 1;
            var result = await Behaviours[Index - 1].ExecuteAsync(request, FinalExecutionAsync, CancellationToken);
            return result;
        }

        throw new IndexOutOfRangeException("No more steps");
    }
}