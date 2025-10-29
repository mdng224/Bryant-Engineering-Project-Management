namespace App.Application.Abstractions.Handlers;

public interface IQueryHandler<in TRequest, TResponse>
{
    Task<TResponse> Handle(TRequest request, CancellationToken ct);
}