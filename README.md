# OrderService — desk review task

You are given a small event-driven microservice on Azure Service Bus. The API
accepts orders, persists them, and publishes OrderCreated; a worker consumes the
event and charges the payment. Review the repository as you would a desk review.
Identify what you would change before this ships to production, and state the
consequence of each issue. We will discuss your findings together. ~10-15 min.

## Build and run

Requires the .NET 8 SDK. Run these commands from the repository root:

```sh
dotnet restore OrderService.sln
dotnet build OrderService.sln --no-restore
dotnet run --project src/OrderService.Api -- --urls http://localhost:5080
```

In another terminal, also from the repository root:

```sh
dotnet run --project src/OrderService.Worker
```

Both hosts use the same local SQLite database, `orders.db`, and create the schema
on startup. For this exercise, start the API before the worker. To use a different
location, set `ConnectionStrings__Orders` to the same absolute SQLite connection
string in both hosts, for example `Data Source=/tmp/orderservice.db`.

```sh
curl -i http://localhost:5080/orders \
  -H 'Content-Type: application/json' \
  -d '{"customerId":"7e4642ed-93fb-4d6c-a6cc-5d1ef2d4d7a1","amount":42.50}'
```

Without Azure configuration, the API injects a logging `IBus` stub and the worker
starts without receiving messages. The stub does not transfer events between
processes. `IPaymentGateway` is also a logging stub; no money is moved.

## Optional Azure Service Bus connection

Set `ServiceBus__ConnectionString` and `ServiceBus__QueueName` in both hosts.
The queue name defaults to `order-events`. Provision a standard, non-session
queue before starting the hosts. Keep credentials outside the repository.
When configured, the API uses `Azure.Messaging.ServiceBus` to publish and the
worker receives with PeekLock and explicit settlement.

Events use JSON bodies, a `ContentType` of `application/json`, a `MessageId`
equal to the event's `EventId`, and a `Subject` matching the contract type:

```json
{
  "EventId": "19cb44f0-5514-42a4-97a3-4fdb48f9bd56",
  "OrderId": "89bdacdc-886e-407a-b31c-58586851e7b5",
  "Amount": 42.50
}
```

The API publishes `OrderCreated`. A payment provider can publish
`PaymentSettled` to the same queue, with `EventId` and `OrderId` in the body and
`PaymentSettled` as the subject. Use an order ID returned by the API.
The worker records payment attempts and applies the order's payment status.

## Source map

| Project | Responsibility |
| --- | --- |
| `OrderService.Api` | Order endpoint and event publication |
| `OrderService.Worker` | Event dispatch, payment processing, and message settlement |
| `OrderService.Contracts` | Shared event contracts |
| `OrderService.Infrastructure` | SQLite persistence, entities, and bus implementations |
