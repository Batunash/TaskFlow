using System;
using System.Collections.Generic;
using System.Text;

namespace TaskFlow.Infrastructure.Identity
{
    public class JwtSettings
    {
        public string Secret { get; init; } = string.Empty;
        public string Issuer { get; init; } = string.Empty;
        public string Audience { get; init; } = string.Empty;
        public int ExpirationMinutes { get; init; }
    }
}
