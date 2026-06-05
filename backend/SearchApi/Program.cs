using Elastic.Clients.Elasticsearch;
using Shared.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var elasticUrl = builder.Configuration["ElasticSearch:Url"] ?? "http://localhost:9200";
var settings = new ElasticsearchClientSettings(new Uri(elasticUrl))
    .DefaultIndex("events");

var client = new ElasticsearchClient(settings);
builder.Services.AddSingleton(client);

// Enable CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();

app.MapGet("/api/events", async (ElasticsearchClient client) =>
{
    var response = await client.SearchAsync<Event>(s => s
        .Query(q => q.MatchAll())
    );

    if (!response.IsValidResponse) return Results.Problem(response.DebugInformation);

    return Results.Ok(response.Documents);
})
.WithName("GetEvents")
.WithOpenApi();

app.MapGet("/api/events/search", async (string q, ElasticsearchClient client) =>
{
    if (string.IsNullOrWhiteSpace(q))
    {
        var allResponse = await client.SearchAsync<Event>(s => s.Query(mq => mq.MatchAll()));
        return Results.Ok(allResponse.Documents);
    }

    var response = await client.SearchAsync<Event>(s => s
        .Query(query => query
            .Match(m => m
                .Field(f => f.Name)
                .Query(q)
            )
        )
    );

    if (!response.IsValidResponse) return Results.Problem(response.DebugInformation);

    return Results.Ok(response.Documents);
})
.WithName("SearchEvents")
.WithOpenApi();

app.Run();
