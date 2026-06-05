# Event Search POC

Esta é uma Prova de Conceito (POC) de busca de eventos utilizando arquitetura baseada em eventos, .NET 8, React, PostgreSQL, Kafka, Debezium e Elasticsearch.

## Arquitetura

1.  **PostgreSQL**: Fonte da verdade para os eventos.
2.  **Debezium**: Captura mudanças no PostgreSQL (CDC) e envia para o Kafka.
3.  **Kafka**: Barramento de mensagens.
4.  **Indexer Worker (.NET)**: Consome mensagens do Kafka e indexa no Elasticsearch.
5.  **Elasticsearch**: Motor de busca com analyzer `edge_ngram` para autocomplete.
6.  **Search API (.NET)**: API de consulta ao Elasticsearch.
7.  **Frontend (React)**: Interface do usuário com busca em tempo real (debounce).

## Como Rodar

### Pré-requisitos

*   Docker e Docker Compose instalados.

### Passos

1.  Navegue até a pasta `docker`:
    ```bash
    cd docker
    ```

2.  Inicie todos os serviços:
    ```bash
    docker-compose up -d --build
    ```

3.  Aguarde alguns instantes para que todos os serviços subam. O serviço `register-connector` irá configurar automaticamente o Debezium.

4.  Acesse as aplicações:
    *   **Frontend**: [http://localhost:3000](http://localhost:3000)
    *   **Search API (Swagger)**: [http://localhost:5000/swagger](http://localhost:5000/swagger)
    *   **Elasticsearch**: [http://localhost:9200](http://localhost:9200)

## Testando o Autocomplete

No Frontend, digite "Cold" na barra de busca. Você deverá ver o evento "Coldplay Music of the Spheres Tour" instantaneamente.

## Inserindo Novos Dados

Para testar o fluxo CDC, insira um novo registro no PostgreSQL:

```sql
docker exec -it postgres psql -U user -d events_db -c "INSERT INTO events (name) VALUES ('Iron Maiden Future Past Tour 2024');"
```

O Debezium capturará a mudança, o Kafka a transportará, o Indexer Worker a processará e em segundos o novo evento estará disponível para busca no Frontend.

## Estrutura do Repositório

```text
/backend
 ├── SearchApi      # API de Consulta
 ├── IndexerWorker  # Worker de Indexação
 └── Shared         # Modelos Compartilhados

/frontend
 └── ReactApp       # Aplicação React/Vite

/docker
 ├── docker-compose.yml
 └── postgres-init  # Script SQL inicial
```
