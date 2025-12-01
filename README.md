# Order – Kafka Outbox Demo

This repository demonstrates an event-driven Order workflow using:

- ASP.NET Core services
- SQL Server with the Outbox pattern
- MassTransit + Apache Kafka for messaging
- A 3-broker Kafka cluster (`kafka1`, `kafka2`, `kafka3`)
- AKHQ for Kafka UI and inspection

## Architecture overview

### Order creation and Outbox pattern

1. A client sends a `POST /api/orders` request to the **Order API**.
2. The Order service:
   - Saves the new Order into **SQL Server**.
   - In the same database transaction, inserts an **Outbox** row representing an `OrderCreated` event.
3. The HTTP request completes only after both the Order and its Outbox event are committed.

This ensures that the write to the database and the intent to publish an event cannot get out of sync.

### OrderJobs → Kafka

- The **OrderJobs** service is a background worker that:
  - Periodically reads unprocessed records from the Outbox table in SQL Server.
  - For each outbox record, publishes an `OrderCreated` message to Kafka on the topic `order-created-topic`.
  - Marks the outbox record as processed after a successful publish.

This implements the Outbox pattern: it guarantees that every committed Order will eventually produce a corresponding Kafka event, even if the Kafka broker or the service is temporarily unavailable.

### PharmacyOrder consumer

- The **PharmacyOrder** service subscribes to the `order-created-topic` Kafka topic.
- When it receives an `OrderCreated` event, it:
  - Creates or updates a pharmacy-side representation of the order.
  - Can trigger additional workflows such as notifications, fulfillment, or further domain processing.

This decouples the Order API from PharmacyOrder: they communicate only via Kafka messages.

## Kafka cluster and docker-compose

The repository includes a `docker-compose.yml` that starts:

- `zookeeper`
- `kafka1`, `kafka2`, `kafka3` – three Kafka brokers forming a cluster
- `akhq` – a Kafka UI for inspecting brokers, topics, and messages

High-level setup:

- Each broker has:
  - A unique `KAFKA_BROKER_ID` (1, 2, 3).
  - Internal listeners for broker-to-broker and AKHQ communication.
  - External listeners mapped to localhost ports (e.g. `19092`, `19093`, `19094`) for local .NET apps.
- The cluster is configured so internal Kafka topics (like `__consumer_offsets`) and the main business topic can use a **replication factor of 3** for fault tolerance.

### Starting the cluster

From the repository root:

