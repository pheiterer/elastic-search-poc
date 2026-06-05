using Confluent.Kafka;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.IndexManagement;
using Elastic.Transport;
using Shared.Models;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Net.Http;

namespace IndexerWorker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IConfiguration _configuration;
    private readonly ElasticsearchClient _elasticClient;

    public Worker(ILogger<Worker> logger, IConfiguration configuration, ElasticsearchClient elasticClient)
    {
        _logger = logger;
        _configuration = configuration;
        _elasticClient = elasticClient;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await EnsureIndexExistsAsync();

        var config = new ConsumerConfig
        {
            BootstrapServers = _configuration["Kafka:BootstrapServers"] ?? "localhost:9092",
            GroupId = "indexer-worker-group-v2",
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        using var consumer = new ConsumerBuilder<string, string>(config).Build();
        consumer.Subscribe("postgres.public.events");

        _logger.LogInformation("Worker started, consuming from topic: postgres.public.events");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var consumeResult = consumer.Consume(stoppingToken);
                if (consumeResult?.Message?.Value == null) continue;

                _logger.LogInformation($"Received message: {consumeResult.Message.Value}");

                var debeziumEvent = JsonSerializer.Deserialize<DebeziumEvent>(consumeResult.Message.Value);

                if (debeziumEvent?.Payload?.After != null)
                {
                    var data = debeziumEvent.Payload.After;
                    var @event = new Event { Id = data.Id, Name = data.Name };
                    var response = await _elasticClient.IndexAsync(@event, i => i.Index("events").Id(@event.Id));

                    if (response.IsValidResponse)
                    {
                        _logger.LogInformation($"Indexed event: {@event.Id} - {@event.Name}");
                    }
                    else
                    {
                        _logger.LogError($"Failed to index event: {response.ApiCallDetails.DebugInformation}");
                    }
                }
                else if (debeziumEvent?.Payload?.Op == "d" && debeziumEvent.Payload.Before != null)
                {
                    // Handle delete
                    var response = await _elasticClient.DeleteAsync<Event>(debeziumEvent.Payload.Before.Id, d => d.Index("events"));
                    if (response.IsValidResponse)
                    {
                        _logger.LogInformation($"Deleted event: {debeziumEvent.Payload.Before.Id}");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Kafka message");
            }
        }

        consumer.Close();
    }

    private async Task EnsureIndexExistsAsync()
    {
        var existsResponse = await _elasticClient.Indices.ExistsAsync("events");

        if (!existsResponse.Exists)
        {
            _logger.LogInformation("Creating 'events' index with custom analyzer...");

            var json = @"
            {
              ""settings"": {
                ""analysis"": {
                  ""tokenizer"": {
                    ""autocomplete_tokenizer"": {
                      ""type"": ""edge_ngram"",
                      ""min_gram"": 1,
                      ""max_gram"": 20,
                      ""token_chars"": [""letter"", ""digit""]
                    }
                  },
                  ""analyzer"": {
                    ""autocomplete"": {
                      ""type"": ""custom"",
                      ""tokenizer"": ""autocomplete_tokenizer"",
                      ""filter"": [""lowercase""]
                    }
                  }
                }
              },
              ""mappings"": {
                ""properties"": {
                  ""id"": { ""type"": ""integer"" },
                  ""name"": { 
                    ""type"": ""text"", 
                    ""analyzer"": ""autocomplete"", 
                    ""search_analyzer"": ""standard"" 
                  }
                }
              }
            }";

            var response = await _elasticClient.Transport.RequestAsync<StringResponse>(
                Elastic.Transport.HttpMethod.PUT, 
                "/events", 
                PostData.String(json));

            if (response.ApiCallDetails.HasSuccessfulStatusCode)
            {
                _logger.LogInformation("'events' index created successfully.");
            }
            else
            {
                _logger.LogError($"'Failed to create index: {response.ApiCallDetails.DebugInformation}");
            }
        }
    }
}

public class DebeziumEvent
{
    [JsonPropertyName("payload")]
    public DebeziumPayload? Payload { get; set; }
}

public class DebeziumPayload
{
    [JsonPropertyName("before")]
    public DebeziumEventData? Before { get; set; }

    [JsonPropertyName("after")]
    public DebeziumEventData? After { get; set; }

    [JsonPropertyName("op")]
    public string? Op { get; set; }
}

public class DebeziumEventData
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}
