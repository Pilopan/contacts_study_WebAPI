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
            builder.Services.AddScoped<IEmailSender, EmailSender>();
            builder.Services.AddScoped<MessageFactory>();
            builder.Services.AddScoped<NetworkClient>();
            builder.Services.AddScoped<DbContext>();
            builder.Services.AddScoped<Repository>();

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

            apiGroup.MapGet("/sendEmailMe/{userName}", SendEmail);

            apiGroup.MapGet("/GetDbRowCount", GetRowCount);

            string SendEmail(string userName, IEmailSender emailSender)
            {
                emailSender.SendEmail(userName);
                return $"Emailt sent to {userName}";
            }

            static string GetRowCount(DbContext dbContext, Repository repository)
            {
                var dbContextRowCount = dbContext.RowCount;
                var repositoryRowCount = repository.rowCount;

                return $" оличество строк dbContextRowCount {dbContextRowCount}, repositoryRowCount {repositoryRowCount}";
            }

            
            app.Run();
        }
        public interface IEmailSender
        {
            public void SendEmail(string userName);
        }
        public class EmailSender : IEmailSender
        {
            private readonly MessageFactory _messageFactory;
            private readonly NetworkClient _networkClient;
            public EmailSender(MessageFactory messageFactory, NetworkClient networkClient)
            {
                _messageFactory = messageFactory;
                _networkClient = networkClient;
            }  
            public void SendEmail(string userName)
            {
                var email = _messageFactory.CreateEmail(userName);
                _networkClient.SendEmail(email);
            }
        }
        public class MessageFactory
        {
            public string CreateEmail(string userName)
            {
                return $"Email to {userName}";
            }
        }
        public class NetworkClient
        {
            public void SendEmail(string email)
            {
                Console.WriteLine($"ќтправлено: {email}");
            }
        }
        class Repository
        {
            readonly private DbContext _dbContext;
            public Repository(DbContext dbContext)
            {
                _dbContext = dbContext;
            }
            public int rowCount => _dbContext.RowCount;
        }

        class DbContext
        {
            public int RowCount { get; } = Random.Shared.Next(0, 1_000_000_000);
        }
    }
}
