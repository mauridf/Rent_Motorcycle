using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Rent_Motorcycle.Utils
{
    public class FileUploadOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var fileUploadAttribute = context.MethodInfo.GetCustomAttributes(true)
                .SingleOrDefault(attr => attr is FileUploadAttribute) as FileUploadAttribute;

            if (fileUploadAttribute != null)
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
                                Properties =
                            {
                                ["imagem"] = new OpenApiSchema { Type = "string", Format = "binary" }
                            }
                            }
                        }
                    }
                };
            }
        }
    }

}
