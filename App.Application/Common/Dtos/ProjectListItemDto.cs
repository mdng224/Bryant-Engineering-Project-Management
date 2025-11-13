namespace App.Application.Common.Dtos;

public sealed record ProjectListItemDto(
    Guid Id,
    Guid ClientId,
    string ClientName,
    Guid ScopeId,
    string ScopeName,
    string Name,
    string Code,
    int Year,
    int Number,
    string Manager,
    string Type,
    string Location,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    DateTimeOffset? DeletedAtUtc,
    Guid? CreatedById,
    Guid? UpdatedById,
    Guid? DeletedById
    );