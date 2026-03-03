using Fudie.DependencyInjection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Fudie.Security.Http;

[Injectable(ServiceLifetime.Singleton, ServiceType = typeof(IHttpContextAccessor))]
public sealed class FudieHttpContextAccessor : HttpContextAccessor;
