namespace App.Application.Abstractions;

public interface ICommandHandler<in TRequest, TResponse>
{
    Task<TResponse> Handle(TRequest request, CancellationToken ct);
}