using Hangfire;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OnlineChatMvc.Data;
using OnlineChatMvc.Hubs;
using OnlineChatMvc.Services;

namespace OnlineChatMvc
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);


            // Add services to the container.
            var connectionString = builder.Configuration.GetConnectionString("ChatConnectionString");

            builder.Services.AddDbContext<ChatContext>(options => options.UseSqlite(connectionString));

            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/login";
                });

            builder.Services.AddAuthorization();

            // Add services to the container.
            builder.Services.AddControllersWithViews();


            builder.Services.AddHangfire(configuration => configuration
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseInMemoryStorage());

            builder.Services.AddHangfireServer();


            builder.Services.AddSignalR();

            builder.Services.AddScoped<ChatService>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                //Authorization = new[] { new BasicAuthAuthorizationFilter(new BasicAuthAuthorizationFilterOptions
                //{
                //    RequireSsl = false,
                //    SslRedirect = false,
                //    LoginCaseSensitive = true,
                //    Users = new[]
                //    {
                //        new BasicAuthAuthorizationUser
                //        {
                //            Login = "sportsoft",
                //            PasswordClear = "hangfire"
                //        }
                //    }
                //}) }

            });
            

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.MapHub<ChatHub>("/chat");

           
            var scope = app.Services.CreateScope();
            var chatService = scope.ServiceProvider.GetRequiredService<ChatService>();
            RecurringJob.AddOrUpdate("DeleteOldMessages", () => chatService.DeleteOldMessages(), "* * * * *");

            app.Run();
        }
    }
}