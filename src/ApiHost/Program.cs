using AiItemSuggestions.Infrastructure;
using ToDoLists.Infrastructure;
using Users.Infrastructure;
using Wolverine;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

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
builder.Services.AddAiItemSuggestionsInfrastructureServices(connectionString);

// Configure Wolverine once for all modules
builder.Host.UseWolverine(opts =>
{
    // Auto-discover message handlers in all modules
    opts.Discovery.IncludeAssembly(typeof(Users.Application.AssemblyMarker).Assembly);
    opts.Discovery.IncludeAssembly(typeof(ToDoLists.Application.AssemblyMarker).Assembly);
    opts.Discovery.IncludeAssembly(typeof(AiItemSuggestions.Application.AssemblyMarker).Assembly);

    // opts.Durability.Mode = DurabilityMode.MediatorOnly;

    if (builder.Environment.IsDevelopment())
    {
        // Optimize Wolverine for usage as if there would never be more than one node running
        opts.Durability.Mode = DurabilityMode.Solo;
    }
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
