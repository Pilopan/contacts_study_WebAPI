using System.Collections.Concurrent;
using contacts.Model;
using Microsoft.AspNetCore.Mvc;
using contacts.Services;
using human_resources.Model;
using human_resources.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography.X509Certificates;

namespace human_resources
{
    public class Program
    {

        public static void Main(string[] args)
        {
            //

            //var _persons = new ConcurrentDictionary<int, Person>();

            //var builder = WebApplication.CreateBuilder(args);
            //var app = builder.Build();

            ////CRUD = Create Read Update Delete


            //var group = app.MapGroup("/Person");
            //group.AddEndpointFilterFactory(ValidationHelpers.ValidateIdFactory);
            //group.AddEndpointFilter<IdValidationFilter>();
            ////Create
            //group.MapPost("/add", (Person person) =>
            //    _persons.TryAdd(person.Id, person) ?
            //        TypedResults.Created("/add", person)
            //        : Results.ValidationProblem(new Dictionary<string, string[]>
            //        {
            //            {person.Id.ToString(), new[] {"A person with this ID already exists"} }
            //        }));
            ////Read (All)
            //group.MapGet("/GetAll", () => _persons);

            ////Read (Id)
            //group.MapGet("/GetById/{id}", (int id) => _persons.TryGetValue(id, out Person person) ?
            //                                            TypedResults.Ok<Person>(person) :
            //                                            Results.NotFound());
            ////Update
            //group.MapPut("/{id}", (Person person, int id) =>
            //{
            //    _persons[id] = person;
            //});

            ////Delete
            //group.MapDelete("/{id}", (int id) => _persons.TryRemove(id, out Person person) ?
            //                                                TypedResults.Ok<Person>(person) :
            //                                                Results.NotFound());
            //app.Run();
            ConcurrentDictionary<int, Contact> _contact = new ConcurrentDictionary<int, Contact>();

            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            //—оздание группы /api
            var apiGroup = app.MapGroup("/api");


            //GET /api/contacts Ч получить все контакты.
            apiGroup.MapGet("/contacts", () => _contact).
                WithName("Contacts");

            //GET /api/contacts/{id} Ч получить контакт по ID.
            apiGroup.MapGet("/contacts/{id:int}", (int id) => _contact.TryGetValue(id, out Contact contact) ?
                                                          TypedResults.Ok<Contact>(contact) :
                                                          Results.NotFound());

            //POST /api/contacts Ч создать новый контакт.
            apiGroup.MapPost("/contacts", (ContactRequest contactRequest, [FromHeader(Name = "Content-Type")] string contentType) => 
            {
                Contact contact = new Contact() 
                {
                    Id = IdGenereationService.GenerateNextIdValue(_contact),
                    FirstName = contactRequest.FirstName,
                    LastName = contactRequest.LastName,
                    Email = contactRequest.Email,
                    Phone = contactRequest.Phone,
                    ContentType = contentType
                };
                //ѕолучим очереной ID контакта
                return _contact.TryAdd(contact.Id, contact) ?
                    TypedResults.Created("/contacts", contact) :
                    Results.ValidationProblem(new Dictionary<string, string[]>
                    {
                        {contact.Id.ToString(), new[] {"A person with this ID already exists"} }
                    });
            });
             
            //PUT /api/contacts/{id} Ч обновить существующий контакт.
            apiGroup.MapPut("/contacts/{id:int}", ([FromRoute] int id, ContactRequest contactRequest, [FromHeader (Name = "Content-Type")] string contentType) => 
            {
                Contact contact = new Contact()
                {
                    Id = id,
                    FirstName = contactRequest.FirstName,
                    LastName = contactRequest.LastName,
                    Email = contactRequest.Email,
                    Phone = contactRequest.Phone,
                    ContentType = contentType
                };
                _contact[id] = contact;
                return TypedResults.Ok(contact);
            });

            //DELETE /api/contacts/{id} Ч удалить контакт.
            apiGroup.MapDelete("/contacts/{id:int}", (int id) => _contact.TryRemove(id, out Contact contact) ?
                                                             TypedResults.Ok(contact) :
                                                             Results.NotFound());

            apiGroup.MapGet("/links", (LinkGenerator links) => 
            {
                string link = links.GetUriByName("Contacts", new { },"https", new HostString("localhost:7135"));
                return $"View the product at {link}";
            });

            apiGroup.MapGet("/test1", () => Results.Redirect(new Uri("https://google.com").ToString())).WithName("test1");
            apiGroup.MapGet("/test2", () => Results.Redirect("test1")).WithName("test2");
            apiGroup.MapGet("/test3", (HttpRequest httpRequest, TestModel testModel) => 
            {
                var validationResults = new List<ValidationResult>();
                var isValid = Validator.TryValidateObject(testModel, new ValidationContext(testModel), validationResults, true);
            });


            app.Run();
        }
        public record TestModel
        {
            [Required]
            [MaxLength(100)]
            public string Name { get; set; }
        }
    }
}
