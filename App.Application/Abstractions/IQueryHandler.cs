namespace App.Application.Abstractions;
public interface IQueryHandler<TRequest, TResponse>
{
    Task<TResponse> Handle(TRequest request, CancellationToken ct);
}