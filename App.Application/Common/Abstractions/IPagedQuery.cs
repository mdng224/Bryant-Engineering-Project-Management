namespace App.Application.Common.Abstractions;

public interface IPagedQuery
{
    int Page { get; }
    int PageSize { get; }
}