namespace App.Application.Abstractions.Handlers;

public interface ICommandHandler<in TRequest, TResponse>
{
    Task<TResponse> Handle(TRequest request, CancellationToken ct);
}