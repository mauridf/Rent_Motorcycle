using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Rent_Motorcycle.Utils
{
    public class FormFileUploadDocumentFilter : IDocumentFilter
    {
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            var schema = new OpenApiSchema
            {
                Type = "object",
                Properties =
                {
                    ["imagem"] = new OpenApiSchema { Type = "string", Format = "binary" }
                }
            };

            swaggerDoc.Components.Schemas.Add("FileUpload", schema);
        }
    }
}