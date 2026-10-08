using FHIR_IHE_API.Data;
using FHIR_IHE_API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FHIR_IHE_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "ProviderOnly")]
    public class AuditController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        private static readonly HashSet<string> AllowedActions =
            new(StringComparer.OrdinalIgnoreCase)
            {
                "READ",
                "CREATE",
                "UPDATE",
                "DELETE",
                "LOGIN_SUCCESS",
                "LOGIN_FAILED"
            };

        private static readonly HashSet<string> AllowedResourceTypes =
            new(StringComparer.OrdinalIgnoreCase)
            {
                "Patient",
                "Authentication",
                "Audit"
            };

        public AuditController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAuditLogs([FromQuery] AuditSearchRequest request)
        {
            // Validate the requested page number.
            const int maxPage = 100000;

            if (request.Page < 1)
            {
                return BadRequest(
                    "Page must be greater than or equal to 1.");
            }

            if (request.Page > maxPage)
            {
                return BadRequest(
                    $"Page cannot exceed {maxPage}.");
            }

            // Validate the requested page size.
            const int maxPageSize = 200;

            if (request.PageSize < 1)
            {
                return BadRequest(
                    "PageSize must be greater than or equal to 1.");
            }

            if (request.PageSize > maxPageSize)
            {
                return BadRequest(
                    $"PageSize cannot exceed {maxPageSize}.");
            }

            // Validate the requested audit action.
            if (!string.IsNullOrWhiteSpace(request.Action) &&
                !AllowedActions.Contains(request.Action))
            {
                return BadRequest(
                    "Invalid audit action.");
            }

            // Validate the requested resource type.
            if (!string.IsNullOrWhiteSpace(request.ResourceType) &&
                !AllowedResourceTypes.Contains(request.ResourceType))
            {
                return BadRequest(
                    "Invalid resource type.");
            }

            // Validate the UTC date range.
            if (request.FromUtc.HasValue &&
                request.ToUtc.HasValue)
            {
                if (request.FromUtc.Value >
                    request.ToUtc.Value)
                {
                    return BadRequest(
                        "FromUtc must be earlier than or equal to ToUtc.");
                }

                var range =
                    request.ToUtc.Value -
                    request.FromUtc.Value;

                if (range > TimeSpan.FromDays(365))
                {
                    return BadRequest(
                        "The audit date range cannot exceed 365 days.");
                }
            }

            // Validate the maximum ResourceId length.
            if (!string.IsNullOrWhiteSpace(request.ResourceId) &&
                request.ResourceId.Length > 200)
            {
                return BadRequest(
                    "ResourceId cannot exceed 200 characters.");
            }

            // Normalize the action value to the canonical value
            // defined in the allowed action list.
            string? action = null;

            if (!string.IsNullOrWhiteSpace(request.Action))
            {
                action = AllowedActions.First(x =>
                    string.Equals(
                        x,
                        request.Action,
                        StringComparison.OrdinalIgnoreCase));
            }

            // Normalize the resource type value to the canonical value
            // defined in the allowed resource type list.
            string resourceType = null;

            if (!string.IsNullOrWhiteSpace(request.ResourceType))
            {
                resourceType = AllowedResourceTypes.First(x =>
                    string.Equals(
                        x,
                        request.ResourceType,
                        StringComparison.OrdinalIgnoreCase));
            }

            var query = _context.AuditLogs.AsNoTracking().AsQueryable();


            // Filter by the Patient who performed the action.
            if (request.PatientId.HasValue)
            {
                query = query.Where(x => x.PatientId == request.PatientId.Value);
            }

            // Filter by the Provider who performed the action.
            if (request.ProviderId.HasValue)
            {
                query = query.Where(x => x.ProviderId == request.ProviderId.Value);
            }

            // Filter by the Patient who was the target of the action.
            if (request.TargetPatientId.HasValue)
            {
                query = query.Where(x => x.TargetPatientId == request.TargetPatientId.Value);
            }

            // Filter by audit action such as READ, CREATE, UPDATE, DELETE.
            if (!string.IsNullOrWhiteSpace(request.Action))
            {
                query = query.Where(x => x.Action == action);
            }

            // Filter by resource type such as Patient or Authentication.
            if (!string.IsNullOrWhiteSpace(request.ResourceType))
            {
                query = query.Where(x => x.ResourceType == resourceType);
            }

            // Filter by the FHIR resource ID.
            if (!string.IsNullOrWhiteSpace(request.ResourceId))
            {
                query = query.Where(x => x.ResourceId == request.ResourceId);
            }

            // Filter by HTTP status code.
            if (request.StatusCode.HasValue)
            {
                query = query.Where(x => x.StatusCode == request.StatusCode.Value);
            }

            // Filter by the beginning of the UTC timestamp range.
            if (request.FromUtc.HasValue)
            {
                query = query.Where(x => x.TimestampUtc >= request.FromUtc.Value);
            }

            // Filter by the end of the UTC timestamp range.
            if (request.ToUtc.HasValue)
            {
                query = query.Where(x => x.TimestampUtc <= request.ToUtc.Value);
            }

            // count the total number of records after applying all filters
            var totalCount = await query.CountAsync();

            // calculate the total number of pages
            var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

            // Retrieve only the records belonging to the requested page
            var logs = await query.OrderByDescending(x => x.TimestampUtc)
                .ThenByDescending(x => x.Id)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            // Convert database entities into API response DTOs.
            var items = logs.Select(AuditLogResponse.FromEntity).ToList();

            return Ok(new
            {
                page = request.Page,
                pageSize = request.PageSize,
                totalCount,
                totalPages,
                items,
            });
        }
    }
}
