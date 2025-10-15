namespace App.Application.Abstractions;

public interface ICommandHandler<TRequest, TResponse>
{
    Task<TResponse> Handle(TRequest request, CancellationToken ct);
}