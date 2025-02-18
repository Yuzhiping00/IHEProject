
using FHIR_IHE_API.Data;
using Hl7.Fhir.Rest;
using Microsoft.EntityFrameworkCore;

namespace FHIR_IHE_API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddDbContext<PatientDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));

            });

            builder.Services.AddControllers().AddNewtonsoftJson();

            // Register HttpClientFactory 
            builder.Services.AddHttpClient();

            //Register FhirClient as Scoped(one instance per request)
            builder.Services.AddScoped<FhirClient>(serviceProvider =>
            {
                var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
                var httpClient = httpClientFactory.CreateClient();

                return new FhirClient("http://localhost:8090/fhir", httpClient);

            });

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowVueApp", builder =>
                {
                    builder.WithOrigins("http://localhost:5173").AllowAnyMethod()
                        .AllowAnyHeader();
                });
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseCors("AllowVueApp");

           // app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
