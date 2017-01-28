using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin;
using Owin;
using RealtimeTestApp.Models;

[assembly: OwinStartupAttribute(typeof(RealtimeTestApp.Startup))]
namespace RealtimeTestApp
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            AddAdmin();
            app.MapSignalR();
        }

        private void AddAdmin()
        {
            ApplicationDbContext context = new ApplicationDbContext();

            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));

            if (!roleManager.RoleExists("Admin"))
            {

                // first we create Admin rool   
                var role = new IdentityRole {Name = "Admin"};
                roleManager.Create(role);

                //Here we create a Admin super user who will maintain the website                  

                var user = new ApplicationUser
                {
                    UserName = "admin@gmail.com",
                    FirstName = "admin",
                    LastName = "admin",
                    Email = "admin@gmail.com"
                };

                const string userPwd = "adminadmin";

                var chkUser = userManager.Create(user, userPwd);

                //Add default User to Role Admin   
                if (chkUser.Succeeded)
                {
                   userManager.AddToRole(user.Id, "Admin");
                }
            }
        }
    }
}
