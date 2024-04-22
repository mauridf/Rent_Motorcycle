using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;

namespace Rent_Motorcycle.Services
{
    public class RabbitMQSenderService
    {
        private readonly IConnection _connection;

        public RabbitMQSenderService(IConnection connection)
        {
            _connection = connection;
        }

        // Propriedade para verificar se a conexão está ativa
        public bool IsConnected => _connection != null && _connection.IsOpen;

        public async Task SendMessageAsync(string queueName, string message)
        {
            using (var channel = _connection.CreateModel())
            {
                channel.QueueDeclare(queue: queueName,
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                var body = Encoding.UTF8.GetBytes(message);

                channel.BasicPublish(exchange: "",
                                     routingKey: queueName,
                                     basicProperties: null,
                                     body: body);
            }
        }
    }
}