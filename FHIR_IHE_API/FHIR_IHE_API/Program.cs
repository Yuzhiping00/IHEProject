using FHIR_IHE_API.Data;
using FHIR_IHE_API.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FHIR_IHE_API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ----------------------------------------
            // Database
            // ----------------------------------------

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));

            });

            // ----------------------------------------
            // ASP.NET Core Identity
            // ----------------------------------------

            builder.Services.AddIdentityCore<ApplicationUser>(options =>
                {
                    options.Password.RequireDigit = true;
                    options.Password.RequireLowercase = true;
                    options.Password.RequireUppercase = true;
                    options.Password.RequireNonAlphanumeric = true;
                    options.Password.RequiredLength = 8;

                    options.User.RequireUniqueEmail = true;
                })
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddSignInManager();

            // ----------------------------------------
            // JWT Authentication
            // ----------------------------------------

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    IssuerSigningKey =
                        new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                            System.Text.Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]!))
                };
            });

            // ----------------------------------------
            // Authorization
            // ----------------------------------------

            builder.Services.AddAuthorization();


            // ----------------------------------------
            // Controllers
            // ----------------------------------------

            builder.Services.AddControllers().AddNewtonsoftJson();

            // ----------------------------------------
            // Swagger
            // ----------------------------------------


            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // ----------------------------------------
            // CORS
            // ----------------------------------------

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowVueApp", builderNew =>
                {
                    builderNew.WithOrigins("http://localhost:5173").AllowAnyMethod()
                        .AllowAnyHeader();
                });
            });

            var app = builder.Build();

            // ----------------------------------------
            // Development tools
            // ----------------------------------------

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // ----------------------------------------
            // Middleware
            // ----------------------------------------

            app.UseCors("AllowVueApp");

            //app.UseHttpsRedirection();

            app.UseAuthentication();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
