

var builder = DistributedApplication.CreateBuilder(args);

// Parameters:
var redisHost = builder.AddParameter("REDISHOST", value: "localhost");
var redisPassword = builder.AddParameter("REDISPASSWORD", value: "yourpassword");
var pgUsername = builder.AddParameter("PGUSER", value: "postgres");
var pgPassword = builder.AddParameter("PGPASSWORD", value: "yourpassword");
var rabbitMqUser = builder.AddParameter("RABBITMQUSER", value: "guest");
var rabbitMqPassword = builder.AddParameter("RABBITMQPASSWORD", value: "guest");

// Add Redis cache
var cache = builder.AddRedis("redis", port: 6379, password: redisPassword)
    .WithRedisInsight();

// Add the database
var postgres = builder.AddPostgres("postgres", pgUsername, pgPassword, port: 5432)
                      .WithPgAdmin();

var postgresdb = postgres.AddDatabase("myservicedb");

// Add RabbitMQ message broker
var rabbitmq = builder.AddRabbitMQ("rabbitmq", rabbitMqUser, rabbitMqPassword, port: 5672)
    .WithManagementPlugin();

// Add the Api project
builder.AddProject<Projects.MyService_Api>("api")
    .WithReference(postgresdb)
    .WaitFor(postgresdb)
    .WithReference(cache)
    .WaitFor(cache)
    .WithReference(rabbitmq)
    .WaitFor(rabbitmq);

builder.Build().Run();
