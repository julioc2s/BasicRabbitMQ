using RabbitMQ.Client;
using System.Configuration;
using System.Text;

var host = ConfigurationManager.AppSettings["host"];
var port = int.Parse(ConfigurationManager.AppSettings["porta"] ?? "5672");
var user = ConfigurationManager.AppSettings["usuario"];
var pass = ConfigurationManager.AppSettings["senha"];

var factory = new ConnectionFactory
{
    HostName = host,
    Port = port,
    UserName = user,
    Password = pass
};

using var connection = await factory.CreateConnectionAsync();
using var channel = await connection.CreateChannelAsync();

string exchangeName = ConfigurationManager.AppSettings["rota"];
string routingKey = ConfigurationManager.AppSettings["routingkey"];

// Declara o exchange (tipo "direct" por padrão)
await channel.ExchangeDeclareAsync(
    exchange: exchangeName,
    type: ExchangeType.Topic,
    durable: true,
    autoDelete: false,
    arguments: null
);

// Opcional: cria uma fila e a vincula (caso ainda não exista)
//await channel.QueueDeclareAsync(
//    queue: "hello",
//    durable: true,
//    exclusive: false,
//    autoDelete: false,
//    arguments: null
//);

//await channel.QueueBindAsync(
//    queue: "hello",
//    exchange: exchangeName,
//    routingKey: routingKey
//);

Console.WriteLine("=== PRODUTOR RABBITMQ ===");
Console.WriteLine($"Exchange: {exchangeName}");
Console.WriteLine("Digite uma mensagem e pressione [ENTER] para enviar.");
Console.WriteLine("Deixe em branco e pressione [ENTER] para sair.\n");

while (true)
{
    Console.Write("Mensagem: ");
    string? mensagem = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(mensagem))
        break;

    var body = Encoding.UTF8.GetBytes(mensagem);

    await channel.BasicPublishAsync(
        exchange: exchangeName,
        routingKey: routingKey,
        body: body
    );

    Console.WriteLine($"[x] Enviado para exchange '{exchangeName}' → {mensagem}\n");
}

Console.WriteLine("Encerrando produtor...");
