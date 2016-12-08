using RiskTracker.Entities;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
using System.Data.Entity.Migrations;
using System.Collections.Generic;

namespace RiskTracker.Entities
{
    public class AuthRepository : BaseRepository
    {
        private const string adminRoleName = "admin";
        private const string advisorRoleName = "advisor";
        private const string coordinatorRoleName = "coordinator";
        private const string supervisorRoleName = "supervisor";
        private const string managerRoleName = "manager";
        private readonly string[] allUserRoles = { advisorRoleName, coordinatorRoleName, supervisorRoleName, managerRoleName };

        private UserManager<IdentityUser> userManager_;
        private RoleManager<IdentityRole> roleManager_;

        public AuthRepository() : base()
        {
            userManager_ = new UserManager<IdentityUser>(new UserStore<IdentityUser>(context));
            roleManager_ = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
        } // AuthRepository

        public void AddIfNotPresentAdminUser(string username, string password)
        {
            var user = userManager_.FindByName(username);
            if (user != null)
              return;
            
            user = new IdentityUser { UserName = username };
            userManager_.Create(user, password);

            var admin = adminRole();
            var roles = userManager_.GetRoles(user.Id);
            if (!roles.Contains(admin))
                userManager_.AddToRole(user.Id, admin);
        } // RegisterAdminUser

        public IdentityResult RegisterUser(string userName, string password, IList<string> roles)
        {
            IdentityUser user = new IdentityUser { UserName = userName };
            IdentityResult result = userManager_.Create(user, password);
            if (result.Succeeded)
              SetUserRoles(userName, roles);
            return result;
        } // RegisterUser

        public void UpdateUserPassword(string userName, string password) {
          var user = userManager_.FindByName(userName);
          if (user == null)
            return;
          userManager_.RemovePassword(user.Id);
          userManager_.AddPassword(user.Id, password);
        } // UpdateUserPassword

        public void SetAsAdvisor(string userName) {
          setUserRole(userName, advisorRoleName);
        } // SetAsAdvisor
        public void SetAsCoordinator(string userName) {
          setUserRole(userName, coordinatorRoleName);
        } // SetAsCoordinator
        public void SetAsSupervisor(string userName) {
          setUserRole(userName, supervisorRoleName);
        } // SetAsSupervisor
        public void SetAsManager(string userName) {
          setUserRole(userName, managerRoleName);
        } // SetAsManager

        public IList<string> UserRoles(string userName) {
          IdentityUser user = userManager_.FindByName(userName);
          return userManager_.GetRoles(user.Id);
        } // userRoles
        public void SetUserRoles(string userName, IList<string> roles) {
          clearUserRoles(userName);

          foreach (var roleName in allUserRoles)
            if (roles.Contains(roleName))
              setUserRole(userName, roleName);
        } // SetUserRole

        private void setUserRole(string userName, string roleName) {
          IdentityUser user = userManager_.FindByName(userName);
          var roles = userManager_.GetRoles(user.Id);
          var role = userRole(roleName);
          if (!roles.Contains(role))
            userManager_.AddToRole(user.Id, role);
        } // setRole

        private void clearUserRoles(string userName) {
          IdentityUser user = userManager_.FindByName(userName);
          var roles = userManager_.GetRoles(user.Id);
          foreach(var roleName in allUserRoles)
            if (roles.Contains(roleName))
              userManager_.RemoveFromRole(user.Id, roleName);
        } // clearUserRole

        public void DeleteUser(string userName) {
          clearUserRoles(userName);
          IdentityUser user = userManager_.FindByName(userName);
          userManager_.Delete(user);
        }

        public async Task<IdentityUser> FindUser(string userName, string password)
        {
            IdentityUser user = await userManager_.FindAsync(userName, password);
            return user;
        } // FindUser

        public IList<string> FindUserRoles(IdentityUser user)
        {
            return userManager_.GetRoles(user.Id);
        } // FindUserRoles

        public ClientApp FindClientApp(string clientId)
        {
            var advisor = context.ClientApps.Find(clientId);
            return advisor;
        } // FindAdvisor

        public async Task<bool> AddRefreshToken(RefreshToken token)
        {
            var existingTokens = context.RefreshTokens.Where(r => r.Subject == token.Subject && r.ClientId == token.ClientId).ToList();
            foreach(var existing in existingTokens)
                context.RefreshTokens.Remove(existing);

            context.RefreshTokens.Add(token);
            return await context.SaveChangesAsync() > 0;
        } // AddRefreshToken

        public async Task<bool> RemoveRefreshToken(string refreshTokenId)
        {
            var refreshToken = await context.RefreshTokens.FindAsync(refreshTokenId);

            if (refreshToken == null)
                return false;

            context.RefreshTokens.Remove(refreshToken);
            return await context.SaveChangesAsync() > 0;
        } // RemoveRefreshToken

        public async Task<bool> RemoveRefreshToken(RefreshToken refreshToken)
        {
            context.RefreshTokens.Remove(refreshToken);
            return await context.SaveChangesAsync() > 0;
        } // RemoveRefreshToken

        public async Task<RefreshToken> FindRefreshToken(string refreshTokenId)
        {
            var refreshToken = await context.RefreshTokens.FindAsync(refreshTokenId);
            return refreshToken;
        } // FindRefreshToken

        public List<RefreshToken> GetAllRefreshTokens()
        {
            return context.RefreshTokens.ToList();
        } // GetAllRefreshTokens

        public async Task<IdentityUser> FindAsync(UserLoginInfo loginInfo)
        {
            IdentityUser user = await userManager_.FindAsync(loginInfo);
            return user;
        } // FindAsync

        public async Task<IdentityResult> CreateAsync(IdentityUser user)
        {
            var result = await userManager_.CreateAsync(user);
            return result;
        } // CreateAsync

        public async Task<IdentityResult> AddLoginAsync(string userId, UserLoginInfo login)
        {
            var result = await userManager_.AddLoginAsync(userId, login);
            return result;
        } // AddLoginAsync

        public override void Dispose()
        {
            context.SaveChanges();
            userManager_.Dispose();
            roleManager_.Dispose();
            base.Dispose();
        } // Dispose

        private string adminRole() { return userRole(adminRoleName); }
        
        private string userRole(string roleName) {
          var role = roleManager_.FindByName(roleName);
          if (role == null) {
            role = new IdentityRole(roleName);
            var roleResult = roleManager_.Create(role);
          }
          return role.Name;
        } // role
    
    } // AuthRepository
}