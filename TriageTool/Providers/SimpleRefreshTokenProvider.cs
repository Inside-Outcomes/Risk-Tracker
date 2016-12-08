using Microsoft.Owin.Security.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using RiskTracker.Entities;

namespace RiskTracker.Providers
{
    public class SimpleRefreshTokenProvider : IAuthenticationTokenProvider
    {
        public async Task CreateAsync(AuthenticationTokenCreateContext context)
        {
            var clientId = context.Ticket.Properties.Dictionary[ASClient.Id];

            if (string.IsNullOrEmpty(clientId))
                return;

            var refreshTokenId = Guid.NewGuid().ToString("n");

            using (AuthRepository repo = new AuthRepository())
            {
                var refreshTokenLifeTime = context.OwinContext.Get<string>(ASClient.RefreshTokenLifeTime);

                var token = new RefreshToken()
                {
                    Id = Helper.GetHash(refreshTokenId),
                    ClientId = clientId,
                    Subject = context.Ticket.Identity.Name,
                    IssuedUtc = DateTime.UtcNow,
                    ExpiresUtc = DateTime.UtcNow.AddMinutes(Convert.ToDouble(refreshTokenLifeTime))
                };

                context.Ticket.Properties.IssuedUtc = token.IssuedUtc;
                context.Ticket.Properties.ExpiresUtc = token.ExpiresUtc;

                token.ProtectedTicket = context.SerializeTicket();

                var result = await repo.AddRefreshToken(token);
                if (result)
                    context.SetToken(refreshTokenId);
            } // using ...
        } // CreateAsync

        public async Task ReceiveAsync(AuthenticationTokenReceiveContext context)
        {
            var allowedOrigin = context.OwinContext.Get<string>(ASClient.AllowedOrigin);
            context.OwinContext.Response.Headers.Add(Header.AccessControlAllowOrigin, new[] { allowedOrigin });

            string hashedTokenId = Helper.GetHash(context.Token);

            using (AuthRepository repo = new AuthRepository())
            {
                var refreshToken = await repo.FindRefreshToken(hashedTokenId);

                if (refreshToken != null)
                {
                    // Get protectedTicket from refreshToken.class
                    context.DeserializeTicket(refreshToken.ProtectedTicket);
                    var result = await repo.RemoveRefreshToken(hashedTokenId);
                } // if ...
            } // using ...
        } // ReceiveAsync

        public void Create(AuthenticationTokenCreateContext context)
        {
            throw new NotImplementedException();
        } // Create

        public void Receive(AuthenticationTokenReceiveContext context)
        {
            throw new NotImplementedException();
        } // Receive
    } // SimpleRefreshTokenProvider
}