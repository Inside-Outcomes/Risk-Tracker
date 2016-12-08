using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RiskTracker.Providers
{
    class Header
    {
        public const string AccessControlAllowOrigin = "Access-Control-Allow-Origin";
    }

    class ASClient
    {
        public const string AllowedOrigin = "as:clientAllowedOrigin";
        public const string Id = "as:client_id";
        public const string RefreshTokenLifeTime = "as:clientRefreshTokenLifeTime";
    }
}