using Elastic.Clients.Elasticsearch;
using IndexerWorker;

var builder = Host.CreateApplicationBuilder(args);

var elasticUrl = builder.Configuration["ElasticSearch:Url"] ?? "http://localhost:9200";
var settings = new ElasticsearchClientSettings(new Uri(elasticUrl))
    .DefaultIndex("events");

var client = new ElasticsearchClient(settings);
builder.Services.AddSingleton(client);

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
