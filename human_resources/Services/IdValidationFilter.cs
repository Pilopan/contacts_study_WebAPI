
using human_resources.Model;
using System.Reflection;

namespace human_resources.Services
{
    public class IdValidationFilter : IEndpointFilter
    {
        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {
            var personArgument = context.Arguments.OfType<Person>().FirstOrDefault();

            if (personArgument == null)
            {
                return Results.Problem("Person data is required");
            }

            if (personArgument.Id < 0)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]> {
                    { personArgument.Id.ToString(), new[] { "The ID will not less then 0" } }
                });
            }

            return next(context);
        }
    }
}
