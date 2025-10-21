namespace App.Domain.Users.Events;

public sealed record UserRegistered(Guid UserId, string Email, UserStatus Status);
