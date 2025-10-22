using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Configuration;
using System.Text;

var host = ConfigurationManager.AppSettings["host"];
var port = int.Parse(ConfigurationManager.AppSettings["porta"] ?? "5672");
var user = ConfigurationManager.AppSettings["usuario"];
var pass = ConfigurationManager.AppSettings["senha"];
var queueName = ConfigurationManager.AppSettings["fila"];

var factory = new ConnectionFactory
{
    HostName = host,
    Port = port,
    UserName = user,
    Password = pass
};

using var connection = await factory.CreateConnectionAsync();
using var channel = await connection.CreateChannelAsync();

await channel.QueueDeclareAsync(
    queue: queueName,
    durable: false,
    exclusive: false,
    autoDelete: false,
    arguments: null);

Console.WriteLine(" [*] Aguardando mensagens... Pressione CTRL+C para sair.");

var consumer = new AsyncEventingBasicConsumer(channel);

consumer.ReceivedAsync += async (model, ea) =>
{
    var body = ea.Body.ToArray();
    var message = Encoding.UTF8.GetString(body);

    Console.WriteLine($"\n [x] Recebido: {message}");
    Console.Write(" Confirmar mensagem? (s/n): ");
    var key = Console.ReadKey();
    Console.WriteLine();

    if (key.KeyChar == 's' || key.KeyChar == 'S')
    {
        await channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
        Console.WriteLine(" [✓] Mensagem confirmada.");
    }
    else
    {
        await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: true);
        Console.WriteLine(" [↩] Mensagem devolvida à fila.");
    }

    await Task.Yield();
};

await channel.BasicConsumeAsync(
    queue: queueName,
    autoAck: false, // <<< importante: controle manual
    consumer: consumer);

await Task.Delay(Timeout.Infinite);
