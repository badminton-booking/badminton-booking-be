namespace be_rallyhubv2.application.abstractions;

public interface IPermissionService
{
    Task<bool> HasPermissionAsync(Guid userId, string permissionCode, CancellationToken cancellationToken);
}