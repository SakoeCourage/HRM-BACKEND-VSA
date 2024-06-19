using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace HRM_BACKEND_VSA.Extensions
{
    public static class JWTStartupConfig
    {

        internal static void ConfigureJWt(IServiceCollection services, IConfiguration configuration)
        {
            var AppKey = configuration.GetValue<string>("SiteSettings:AppKey");
            var jwtSettings = configuration.GetSection("JwtSettings");

            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(AppKey)),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidAudience = jwtSettings["validAudience"],
                    ValidIssuer = jwtSettings["validIssuer"]

                };
            });
        }
    }
}
