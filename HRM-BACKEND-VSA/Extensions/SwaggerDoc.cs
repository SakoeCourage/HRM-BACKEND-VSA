using System.Reflection;
using System.Text.Json.Serialization;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace HRM_BACKEND_VSA.Extensions
{


    public static class SwaggerDoc
    {
        public static class SwaggerEndpointDefintions
        {
            public static string UserManagement = "User Management";
            public static string Setup = "Setup";
            public static string Planning = "Planning";
            public static string PostingAndTransfer = "Posting and Transfer";
            public static string AppoinmentAndSeparation = "Appointment and Separation";
            public static string ApplicantService = "Applicant Service";
            public static string TestFeature = "Test Features";
            public static string SMSService = "SMS Services";
            public static string StaffRequestHandler = "Staff Request Services";
            public static string HRAuthService = "HR Auth Services";
        }
        
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public enum StaffListFilter
        {
            BANKDETAIL,
            CURRENTAPPOINTMENT,
            FAMILYDETAIL,
            PROFESSIONALLICENSE,
            STAFFCHILDREN,
            STAFFACCOMMODATION,
            STAFFPOSTING
        }
        
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public enum StaffStatus
        {
            ACTIVE,
            INACTIVE
        }

        public enum TransferListFilters
        {
            SEPARATED,
            TRANSFERED
        }
        
        public static List<Type> enumTypes = new List<Type>
        {
            typeof(StaffListFilter),
            typeof(StaffStatus),
            typeof(TransferListFilters)
        };

        public static void MapEnumsToString(SwaggerGenOptions options, IEnumerable<Type> enumTypes)
        {
            foreach (var enumType in enumTypes)
            {
                if (enumType.IsEnum)
                {
                    options.MapType(enumType, () => new OpenApiSchema
                    {
                        Type = "string",
                        Enum = Enum.GetNames(enumType)
                            .Select(name => new OpenApiString(name))
                            .ToList<IOpenApiAny>()
                    });
                }
            }
        }

        public class SwaggerFileOperationFilter : IOperationFilter
        {
            public void Apply(OpenApiOperation operation, OperationFilterContext context)
            {
                var fileParams = context.ApiDescription.ParameterDescriptions
                    .Where(x => x.ModelMetadata.ElementType == typeof(IFormFile))
                    .Select(x => x.Name);

                if (fileParams.Any())
                {
                    operation.RequestBody = new OpenApiRequestBody
                    {
                        Content = new Dictionary<string, OpenApiMediaType>
                        {
                            ["multipart/form-data"] = new OpenApiMediaType
                            {
                                Schema = new OpenApiSchema
                                {
                                    Type = "object",
                                    Properties = fileParams.ToDictionary(param => param,
                                        param => new OpenApiSchema { Type = "string", Format = "binary" }),
                                    Required = fileParams.ToHashSet()
                                }
                            }
                        }
                    };
                }
            }
        }

        public static void OpenAuthentication(SwaggerGenOptions option)
        {
            option.SwaggerDoc("v1", new OpenApiInfo { Title = "HRM-SK API", Version = "v1" });
            var endpointDefinitions = typeof(SwaggerEndpointDefintions).GetFields(BindingFlags.Public | BindingFlags.Static);
            foreach (var definitions in endpointDefinitions)
            {
                option.SwaggerDoc(definitions.GetValue(null)?.ToString(), new OpenApiInfo { Title = "HRM-SK API", Version = "v1" });
            }
            
            MapEnumsToString(option, enumTypes);

            option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Description = "Please enter a valid token",
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                BearerFormat = "JWT",
                Scheme = "Bearer"
            });
            option.OperationFilter<SwaggerFileOperationFilter>();
            option.SchemaFilter<EnumSchemaFilter>();
            option.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    new string[] { }
                }
            });
            
            option.DocInclusionPredicate((documentName, apiDescription) => apiDescription.GroupName == documentName);
        }
    }

    public class EnumSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema model, SchemaFilterContext context)
        {
            if (context.Type.IsEnum)
            {
                model.Enum.Clear();
                Enum.GetNames(context.Type)
                    .ToList()
                    .ForEach(name => model.Enum.Add(new OpenApiString($"{name}")));
            }
        }
    }
}