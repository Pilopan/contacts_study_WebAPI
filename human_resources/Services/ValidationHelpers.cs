using human_resources.Model;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace human_resources.Services
{
    public class ValidationHelpers
    {
        public static async ValueTask<object?> ValidateID(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {
            //var id = context.GetArgument<int>(0);
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
        
        internal static EndpointFilterDelegate ValidateIdFactory(EndpointFilterFactoryContext context, EndpointFilterDelegate next)
        {
            ParameterInfo[] parameters = context.MethodInfo.GetParameters();

            int? personPsition = null;
            int? idPosition = null;
            if (parameters.Length > 0)
            {
                for (int i = 0; i < parameters.Length; i++)
                {
                    var parameter = parameters[i];
                    if (parameter.ParameterType == typeof(Person))
                    {
                        personPsition = i; 
                    }
                    if (parameter.ParameterType == typeof(int) && parameter.Name == "id")
                    {
                        idPosition = i;
                    }
                }
            }
            if (!personPsition.HasValue && !idPosition.HasValue)
            {
                return next;
            }
            else
            {
                if (personPsition.HasValue && idPosition.HasValue)
                {
                    return async (invocationContext) =>
                    {
                        var id = invocationContext.GetArgument<int>(idPosition.Value);
                        var person = invocationContext.GetArgument<Person>(personPsition.Value);
                        if (person.Id != id)
                        {
                            return Results.ValidationProblem(new Dictionary<string, string[]> {
                                                                { $"{person.Id.ToString()} != {id}", new[] { "Значения ID не равны" } }
                                                            });
                        }
                        if (person.Id < 0)
                        {
                            return Results.ValidationProblem(new Dictionary<string, string[]> {
                                { person.Id.ToString(), new[] { "The ID will not less then 0" } }
                            });
                        }
                        return await next(invocationContext);
                    };
                }                
                if (personPsition.HasValue)
                {
                    return async (invocationContext) =>
                    {
                        var person = invocationContext.GetArgument<Person>(personPsition.Value);
                        if (person.Id < 0) 
                        { 
                            return Results.ValidationProblem(new Dictionary<string, string[]> {
                                { person.Id.ToString(), new[] { "The ID will not less then 0" } }
                            });
                        }
                        return await next(invocationContext);
                    };
                }
                if (idPosition.HasValue) 
                {
                    return async (invocationContext) =>
                    {
                        var id = invocationContext.GetArgument<int>(idPosition.Value);
                        if (id < 0)
                        {
                            return Results.ValidationProblem(new Dictionary<string, string[]> {
                                { id.ToString(), new[] { "The ID will not less then 0" } }
                            });
                        }
                        return await next(invocationContext);
                    };
                }
                return async (invocationContext) =>
                {
                    return await next(invocationContext);
                };
            }    
        }
    }
}
