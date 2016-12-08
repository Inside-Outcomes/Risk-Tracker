using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using Microsoft.Owin.Security.OAuth;
using Microsoft.AspNet.Identity.EntityFramework;
using RiskTracker.Entities;
using System.Security.Claims;
using Microsoft.Owin.Security;

namespace RiskTracker.Providers
{
    public class SimpleAuthorizationServerProvider : OAuthAuthorizationServerProvider
    {
        private const string invalidClientId = "invalid_clientId";
        private const string invalidGrant = "invalid_grant";

        private const string userName = "userName";
        private const string userRole = "userRole";
        
        public override Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            string clientId = string.Empty;
            string clientSecret = string.Empty;
            ClientApp client = null;

            if (!context.TryGetBasicCredentials(out clientId, out clientSecret))
                context.TryGetFormCredentials(out clientId, out clientSecret);

            if (context.ClientId == null)
            {
                context.Validated();
                return clientIdError(context, "ClientId should be sent");
            } // if ...

            using (AuthRepository repo_ = new AuthRepository())
            {
                client = repo_.FindClientApp(context.ClientId);
            }

            if (client == null)
                return clientIdError(context, string.Format("Advisor '{0}' is not registered in the system.", context.ClientId));

            if (client.ApplicationType == Models.ApplicationTypes.NativeConfidential)
            {
                if (string.IsNullOrWhiteSpace(clientSecret))
                    return clientIdError(context, "Advisor secret should be sent.");
                if (client.Secret != Helper.GetHash(clientSecret))
                    return clientIdError(context, "Advisor secret is invalid");
            } // if ...

            if (!client.Active) 
                return clientIdError(context, "Advisor is inactive.");

            context.OwinContext.Set<string>(ASClient.AllowedOrigin, client.AllowedOrigin);
            context.OwinContext.Set<string>(ASClient.RefreshTokenLifeTime, client.RefreshTokenLifeTime.ToString());

            context.Validated();
            return Task.FromResult<object>(null);
        } // ValidateClientAuthentication

        private Task clientIdError<T>(BaseValidatingContext<T> context, string errorMsg)
        {
            context.SetError(invalidClientId, errorMsg);
            return Task.FromResult<object>(null);
        }

        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            var allowedOrigin = context.OwinContext.Get<string>(ASClient.AllowedOrigin);

            if (allowedOrigin == null)
                allowedOrigin = "*";

            context.OwinContext.Response.Headers.Add(Header.AccessControlAllowOrigin, new[] { allowedOrigin });
            
            IdentityUser user = await findUser(context.UserName, context.Password);
            if (user == null)
            {
               context.SetError(invalidGrant, "The username or password is incorrect.");
               return;
            }

            var roles = findUserRoles(user);

            if (!roles.Contains("admin"))
              if (!organisationIsValid(user)) {
                context.SetError(invalidGrant, "Your organisation's account is not active.");
                return;
              }

            var identity = new ClaimsIdentity(context.Options.AuthenticationType);
            identity.AddClaim(new Claim(ClaimTypes.Name, context.UserName));
            identity.AddClaim(new Claim(ClaimTypes.Role, "user"));
            identity.AddClaim(new Claim("sub", context.UserName));

            var props = new AuthenticationProperties(new Dictionary<string, string>
                {
                    { ASClient.Id, (context.ClientId == null) ? string.Empty : context.ClientId },
                    { userName, context.UserName },
                    { userRole, roles }
                });

            var ticket = new AuthenticationTicket(identity, props);
            context.Validated(ticket);
        } // GrantResourceOwnerCredentials

        private async Task<IdentityUser> findUser(string userName, string password)
        {
            using (AuthRepository repo = new AuthRepository())
            {
                IdentityUser user = await repo.FindUser(userName, password);
                return user;
            }
        } // findUser

        private string findUserRoles(IdentityUser user)
        {
            using (AuthRepository repo = new AuthRepository())
            {
                string sb = "";
                foreach (string role in repo.FindUserRoles(user))
                {
                    if (sb.Length != 0)
                        sb += ",";
                    sb += role;
                } // foreach
                return sb;
            } // using
        } // findUserRole

        private bool organisationIsValid(IdentityUser user) {
          using (ProjectOrganisationRepository repo_ = new ProjectOrganisationRepository()) {
            var staff = repo_.FindStaffMember(user.UserName);
            return !staff.Organisation.IsSuspended;
          } // using ...
        } // organisationIsValid

        public override Task GrantRefreshToken(OAuthGrantRefreshTokenContext context)
        {
            var originalClient = context.Ticket.Properties.Dictionary[ASClient.Id];
            var currentClient = context.ClientId;

            if (originalClient != currentClient)
                return clientIdError(context, "Refresh token is issued to a different clientId.");

            // Change auth ticket for refresh token requests
            var newIdentity = new ClaimsIdentity(context.Ticket.Identity);

            var newClaim = newIdentity.Claims.Where(c => c.Type == "newClaim").FirstOrDefault();
            if (newClaim != null)
                newIdentity.RemoveClaim(newClaim);
            newIdentity.AddClaim(new Claim("newClaim", "newValue"));

            var newTicket = new AuthenticationTicket(newIdentity, context.Ticket.Properties);
            context.Validated(newTicket);

            return Task.FromResult<object>(null);
        } // GrantRefreshToken

        public override Task TokenEndpoint(OAuthTokenEndpointContext context)
        {
            foreach (KeyValuePair<string, string> property in context.Properties.Dictionary)
                context.AdditionalResponseParameters.Add(property.Key, property.Value);

            return Task.FromResult<object>(null);
        } // TokenEndpoint
    } // class SimpleAuthorizationServerProvider
}