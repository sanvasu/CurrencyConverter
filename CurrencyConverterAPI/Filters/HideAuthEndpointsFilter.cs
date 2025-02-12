using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

public class HideAuthEndpointsFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        var authPaths = context.ApiDescriptions
            .Where(x => x.RelativePath.Contains("auth") || x.RelativePath.Contains("login"))
            .Select(x => x.RelativePath)
            .ToList();

        foreach (var path in authPaths)
        {
            swaggerDoc.Paths.Remove("/" + path);
        }
    }
}
