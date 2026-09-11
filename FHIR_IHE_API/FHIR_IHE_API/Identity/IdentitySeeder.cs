using FHIR_IHE_API.Data;
using FHIR_IHE_API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FHIR_IHE_API.Identity
{
    public class IdentitySeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

            var context = services.GetRequiredService<ApplicationDbContext>();

            // ----------------------------------------
            // 1. Create roles
            // ----------------------------------------

            string[] roles =
            [
                "Provider",
                "Patient"
            ];

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(
                        new IdentityRole(role));
                }
            }

            // ----------------------------------------
            // 2. Create Provider user
            // ----------------------------------------

            var providerEmail = "doctor@example.com";

            var provider = await userManager.FindByEmailAsync(providerEmail);

            if (provider == null)
            {
                provider = new ApplicationUser
                {
                    UserName = providerEmail,
                    Email = providerEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(provider, "Provider123!");

                if (!result.Succeeded)
                {
                    throw new Exception(string.Join("; ", result.Errors.Select(e => e.Description)));

                }

                await userManager.AddToRoleAsync(provider, "Provider");
            }

            // ----------------------------------------
            // 3. Create a Patient record
            // ----------------------------------------

            var patient = await context.Patients.FirstOrDefaultAsync(p => p.FhirId == "seed-patient-001");

            if (patient == null)
            {
                patient = new Patient
                {
                    FhirId = "seed-patient-001",
                    FamilyName = "Test",
                    GivenName = "Patient",
                    Gender = "male",
                    BirthDate = new DateTime(1990, 1, 1),
                    JsonData = null
                };

                context.Patients.Add(patient);

                await context.SaveChangesAsync();
            }

            // ----------------------------------------
            // 4. Create Patient user
            // ----------------------------------------

            var patientEmail = "patient@example.com";

            var patientUser = await userManager.FindByEmailAsync(patientEmail);

            if (patientUser == null)
            {
                patientUser = new ApplicationUser
                {
                    UserName = patientEmail,
                    Email = patientEmail,
                    EmailConfirmed = true,
                    PatientId = patient.Id
                };

                var result = await userManager.CreateAsync(patientUser, "Patient123!");

                if (!result.Succeeded)
                {
                    throw new Exception(string.Join("; ", result.Errors.Select(e => e.Description)));
                }

                await userManager.AddToRoleAsync(patientUser, "Patient");
            }

            else if (patientUser.PatientId != patient.Id)
            {
                patientUser.PatientId = patient.Id;

                await userManager.UpdateAsync(patientUser);
            }

        }
    }
}
