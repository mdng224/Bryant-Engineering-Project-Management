namespace App.Application.Abstractions;
public interface IQueryHandler<in TRequest, TResponse>
{
    Task<TResponse> Handle(TRequest request, CancellationToken ct);
}