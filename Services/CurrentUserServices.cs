﻿using study4_be.Interface;
using System.Security.Claims;

namespace study4_be.Services
{
    public class CurrentUserServices : ICurrentUserServices
    {
            private readonly IHttpContextAccessor httpContextAccessor;

            public CurrentUserServices(IHttpContextAccessor httpContextAccessor)
            {
                this.httpContextAccessor = httpContextAccessor;
            }
            public string? UserId => httpContextAccessor?.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

            public string? IpAddress => httpContextAccessor?.HttpContext?.Connection?.LocalIpAddress?.ToString();
    }
}
