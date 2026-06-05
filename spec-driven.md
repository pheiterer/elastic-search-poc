Crie uma Prova de Conceito (POC) de busca de eventos utilizando arquitetura baseada em eventos.

## Objetivo

Construir uma aplicação onde o usuário pode visualizar uma lista de eventos e realizar buscas por nome utilizando Elasticsearch.

Cada evento possui apenas:

```csharp
public class Event
{
    public int Id { get; set; }
    public string Name { get; set; }
}
```

Exemplos:

```json
[
  {
    "id": 1,
    "name": "Coldplay Music of the Spheres Tour"
  },
  {
    "id": 2,
    "name": "Taylor Swift Eras Tour"
  }
]
```

---

## Stack

### Frontend

* React
* TypeScript
* Vite
* Axios

### Backend

* ASP.NET Core 8 Web API

### Persistência

* PostgreSQL

### Busca

* Elasticsearch

### Mensageria

* Apache Kafka
* Debezium para CDC

---

## Arquitetura

Os componentes devem ser implementados como serviços independentes.

```text
Frontend (React)
        |
        v
Search API (.NET)
        |
        v
 Elasticsearch

PostgreSQL
     |
     v
Debezium CDC
     |
     v
Kafka
     |
     v
Indexer Worker (.NET)
     |
     v
Elasticsearch
```

### Importante

O Search API e o Indexer Worker são aplicações separadas.

Cada uma deve possuir:

* Projeto próprio
* Dockerfile próprio
* Container próprio no docker-compose

O Search API não deve conter nenhuma lógica de indexação.

O Indexer Worker não deve expor endpoints HTTP.

Sua única responsabilidade é consumir eventos do Kafka e atualizar o Elasticsearch.

---

## Fluxo de Dados

### Carga Inicial

Os dados serão inseridos diretamente no PostgreSQL através de scripts SQL.

Não criar endpoints de CRUD.

### Sincronização

1. Um registro é inserido ou alterado no PostgreSQL.
2. O Debezium captura a mudança utilizando o WAL.
3. O Debezium publica um evento no Kafka.
4. O Indexer Worker consome o evento.
5. O documento é indexado ou atualizado no Elasticsearch.

### Busca

1. O usuário acessa a aplicação React.
2. A tela carrega todos os eventos disponíveis.
3. O usuário digita um texto na barra de pesquisa.
4. O React chama a Search API.
5. A Search API consulta o Elasticsearch.
6. Os resultados são exibidos em tempo real.

---

## Frontend

Criar uma única tela contendo:

### Lista de Eventos

Ao carregar a página, exibir todos os eventos existentes.

### Barra de Pesquisa

Campo de texto para pesquisa por nome.

Exemplo:

```text
[ Coldplay                ]
```

Conforme o usuário digita:

```text
c
co
col
cold
```

novas requisições devem ser realizadas para a API utilizando debounce.

Exemplo:

```http
GET /api/events/search?q=cold
```

A lista deve ser atualizada dinamicamente com os resultados retornados.

### Requisitos da Busca

* Busca parcial por texto
* Busca por palavras incompletas
* Ordenação por relevância
* Atualização da lista sem recarregar a página

---

## Search API

Implementar apenas endpoints de consulta.

```http
GET /api/events
```

Retorna todos os eventos indexados.

```http
GET /api/events/search?q=termo
```

Retorna eventos encontrados.

A API deve consultar apenas o Elasticsearch.

Nunca consultar diretamente o PostgreSQL.

---

## Elasticsearch

Criar índice:

```text
events
```

Mapeamento:

```json
{
  "properties": {
    "id": {
      "type": "integer"
    },
    "name": {
      "type": "text"
    }
  }
}
```

Implementar consultas utilizando:

* Match Query
* Full Text Search
* Ordenação por score

---

## Docker Compose

Criar containers independentes para:

* PostgreSQL
* Zookeeper
* Kafka
* Debezium Connect
* Elasticsearch
* Search API (.NET)
* Indexer Worker (.NET)
* Frontend React

Todos os serviços devem estar configurados para comunicação local através do Docker Compose.

---

## Estrutura Esperada

```text
/backend
 ├── SearchApi
 ├── IndexerWorker
 └── Shared

/frontend
 └── ReactApp

/docker
 └── docker-compose.yml
```

Gerar todos os arquivos necessários para execução local, incluindo Dockerfiles, docker-compose, configurações do Kafka, Debezium, Elasticsearch, Search API, Indexer Worker e Frontend.


## Busca e Autocomplete

A experiência de busca deve ser semelhante à encontrada em sites de ingressos e eventos.

O usuário não precisa digitar o nome completo do evento para encontrar resultados.

Exemplos:

```text
c
co
col
cold
coldp
coldplay
```

Todos os exemplos acima devem retornar:

```text
Coldplay Music of the Spheres Tour
```

### Elasticsearch Analyzer

Configurar o índice utilizando um analyzer baseado em `edge_ngram` para suportar autocomplete e busca por prefixo.

Exemplo de configuração:

```json
{
  "settings": {
    "analysis": {
      "tokenizer": {
        "autocomplete_tokenizer": {
          "type": "edge_ngram",
          "min_gram": 1,
          "max_gram": 20,
          "token_chars": [
            "letter",
            "digit"
          ]
        }
      },
      "analyzer": {
        "autocomplete": {
          "type": "custom",
          "tokenizer": "autocomplete_tokenizer",
          "filter": [
            "lowercase"
          ]
        }
      }
    }
  }
}
```

### Mapeamento

```json
{
  "properties": {
    "id": {
      "type": "integer"
    },
    "name": {
      "type": "text",
      "analyzer": "autocomplete",
      "search_analyzer": "standard"
    }
  }
}
```

### Estratégia de Busca

A Search API deve utilizar consultas otimizadas para autocomplete.

Priorizar:

* Multi Match Query
* Match Query
* Prefix Search
* Relevância por score

Exemplo:

```json
{
  "query": {
    "match": {
      "name": {
        "query": "cold"
      }
    }
  }
}
```

### Frontend

A busca deve acontecer automaticamente enquanto o usuário digita.

Implementar debounce de 300ms para evitar excesso de requisições.

Fluxo:

1. Usuário digita no campo de busca.
2. React aguarda 300ms sem novas teclas.
3. React chama a API.
4. API consulta o Elasticsearch.
5. Lista de eventos é atualizada.

Não deve existir botão "Pesquisar".

A busca deve ser instantânea.

### Performance

A solução deve ser preparada para suportar dezenas de milhares de eventos sem degradação perceptível da experiência de busca.

O PostgreSQL continua sendo a fonte da verdade.

Toda consulta da aplicação deve ser realizada exclusivamente através do Elasticsearch.
