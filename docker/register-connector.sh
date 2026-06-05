#!/bin/bash

echo "Waiting for Debezium to be ready..."
until curl -s http://localhost:8083/ > /dev/null; do
  sleep 5
done

echo "Registering PostgreSQL connector..."
curl -i -X POST -H "Accept:application/json" -H  "Content-Type:application/json" http://localhost:8083/connectors/ -d '{
  "name": "events-connector",
  "config": {
    "connector.class": "io.debezium.connector.postgresql.PostgresConnector",
    "plugin.name": "pgoutput",
    "database.hostname": "postgres",
    "database.port": "5432",
    "database.user": "user",
    "database.password": "password",
    "database.dbname": "events_db",
    "database.server.name": "postgres",
    "topic.prefix": "postgres",
    "table.include.list": "public.events"
  }
}'
