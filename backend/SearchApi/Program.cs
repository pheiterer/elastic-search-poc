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
        .Index("events")
        .Query(q => q.MatchAll())
        .Size(1000)
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
        var allResponse = await client.SearchAsync<Event>(s => s
            .Index("events")
            .Query(mq => mq.MatchAll())
            .Size(1000)
        );
        return Results.Ok(allResponse.Documents);
    }

    var response = await client.SearchAsync<Event>(s => s
        .Index("events")
        .Query(query => query
            .Bool(b => b
                .Should(
                    sh => sh.Match(m => m.Field(f => f.Name).Query(q)),
                    sh => sh.Prefix(p => p.Field(f => f.Name).Value(q.ToLower()).Boost(5.0f))
                )
            )
        )
        .Size(1000)
    );

    if (!response.IsValidResponse) return Results.Problem(response.DebugInformation);

    return Results.Ok(response.Documents);
})
.WithName("SearchEvents")
.WithOpenApi();

app.Run();
