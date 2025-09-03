using ToDoLists.Infrastructure;
using Users.Infrastructure;
using Wolverine;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();
builder.Services.AddProblemDetails();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddControllers();

// Configure database connection
string connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                         throw new Exception("No connection string found.");

// Register module services  
builder.Services.AddUsersInfrastructureServices(connectionString);
builder.Services.AddToDoListsInfrastructureServices(connectionString);


// Configure Wolverine once for all modules
builder.Host.UseWolverine(opts =>
{
    // Auto-discover message handlers in both modules
    opts.Discovery.IncludeAssembly(typeof(Users.Application.AssemblyMarker).Assembly);
    opts.Discovery.IncludeAssembly(typeof(ToDoLists.Application.AssemblyMarker).Assembly);

    // Configure for mediator usage
    opts.Durability.Mode = DurabilityMode.MediatorOnly;
});

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();

// Make Program class accessible for testing
public partial class Program { }
