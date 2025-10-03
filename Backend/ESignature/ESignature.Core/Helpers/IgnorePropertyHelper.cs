using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;
using System.Text.Json.Serialization;

namespace ESignature.Core.Helpers
{
    public class IgnorePropertyHelper : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (context.ApiDescription == null || operation.Parameters == null)
                return;

            if (!context.ApiDescription.ParameterDescriptions.Any())
                return;

            //context.ApiDescription.ParameterDescriptions.Where(p => p.Source.Equals(BindingSource.Form)
            //            && p.CustomAttributes().Any(p => p.GetType().Equals(typeof(JsonIgnoreAttribute))))
            //    .ForAll(p => operation.RequestBody.Content.Values.Single(v => v.Schema.Properties.Remove(p.Name)));
            foreach (var parameter in context.ApiDescription.ParameterDescriptions.Where(p => p.Source.Equals(BindingSource.Form)
                            && p.CustomAttributes().Any(attr => attr.GetType() == typeof(JsonIgnoreAttribute))))
            {
                operation.RequestBody.Content.Values
                    .Single(v => v.Schema.Properties.Remove(parameter.Name));
            }
            //context.ApiDescription.ParameterDescriptions.Where(p => p.Source.Equals(BindingSource.Query)
            //              && p.CustomAttributes().Any(p => p.GetType().Equals(typeof(JsonIgnoreAttribute))))
            //    .ForAll(p => operation.Parameters.Remove(operation.Parameters.Single(w => w.Name.Equals(p.Name))));
            foreach(var parameter in context.ApiDescription.ParameterDescriptions.Where(p => p.Source.Equals(BindingSource.Query)
                          && p.CustomAttributes().Any(p => p.GetType().Equals(typeof(JsonIgnoreAttribute)))))
            {
                operation.Parameters.Remove(operation.Parameters.Single(w => w.Name.Equals(parameter.Name)));
            }
        }
    }
}