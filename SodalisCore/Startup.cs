using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SodalisCore.Repositories;
using SodalisCore.Services;
using SodalisDatabase;

namespace SodalisCore {
    public class Startup {
        public Startup(IConfiguration configuration) {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services) {
            services.AddControllers();

            //TODO move connection string out of code
            services.AddDbContext<SodalisContext>(options => options.UseSqlServer("Server=localhost;database=Sodalis;Integrated Security=SSPI"));
            AddSodalisServices(services);

            AddSodalisSingletons(services);

            services.AddLogging();

            services.AddCors();

            AddSodalisAuthentication(services);
            AddSodalisAuthorization(services);

            services.AddSwaggerGen(context => context.SwaggerDoc("v1", new OpenApiInfo { Title = "Sodalis API", Version = "v1" }));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        // ReSharper disable once UnusedMember.Global
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();
            app.UseSwaggerUI(context => context.SwaggerEndpoint("/swagger/v1/swagger.json", "Sodalis API"));

            app.UseHttpsRedirection();

            app.UseRouting();

            //TODO finetune CORS policy
            app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

            app.UseAuthentication();

            //enables access to request bodies for logging
            app.Use((context, next) => {
                context.Request.EnableBuffering();
                return next();
            });

            app.UseAuthorization();

            app.UseEndpoints(endpoints => {
                endpoints.MapControllers();
            });
        }

        private static void AddSodalisServices(IServiceCollection services) {
            services.AddScoped<ICryptographyService, CryptographyService>();
            services.AddScoped<IClaimService, ClaimService>();
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<IAuthenticationRepository, AuthenticationRepository>();
            services.AddScoped<IGoalService, GoalService>();
            services.AddScoped<IGoalRepository, GoalRepository>();
            services.AddScoped<IFriendService, FriendService>();
            services.AddScoped<IFriendRepository, FriendRepository>();
        }

        private static void AddSodalisSingletons(IServiceCollection services) {
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        }

        private static void AddSodalisAuthentication(IServiceCollection services) {
            //TODO make token string a secret
            var key = Encoding.ASCII.GetBytes("this key shouldn't be in the codebase");

            services.AddAuthentication(options => {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options => {
                options.Events = SetupBearerEvents();
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });
        }

        private static void AddSodalisAuthorization(IServiceCollection services) {
            services.AddAuthorization();
        }

        private static JwtBearerEvents SetupBearerEvents() {
            return new JwtBearerEvents {
                OnTokenValidated = context => {
                    return Task.CompletedTask;
                }
            };
        }
    }
}
