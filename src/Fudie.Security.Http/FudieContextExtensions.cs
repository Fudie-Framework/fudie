using Fudie.Domain;
using Fudie.Security;

namespace Fudie.Security.Http;

public static class FudieContextExtensions
{
    public static Guid RequiredUserId(this IFudieContext context) =>
        context.UserId ?? throw new UnauthorizedException("User authentication is required.");

    public static Guid RequiredTenantId(this IFudieContext context) =>
        context.TenantId ?? throw new UnauthorizedException("Tenant context is required.");

    public static Guid RequiredSessionId(this IFudieContext context) =>
        context.SessionId ?? throw new UnauthorizedException("Session context is required.");

    public static Guid RequiredAppId(this IFudieContext context) =>
        context.AppId ?? throw new UnauthorizedException("Application context is required.");
}
