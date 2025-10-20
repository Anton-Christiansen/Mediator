namespace Mediator.Interfaces;

public interface IBehaviourEnumerator<in TRequest>
{
    public ValueTask ExecuteAsync(TRequest request, CancellationToken cancellationToken);
}

public interface IBehaviourEnumerator<in TRequest, TResponse>
{
    public ValueTask<TResponse> ExecuteAsync(TRequest request, CancellationToken cancellationToken);
}