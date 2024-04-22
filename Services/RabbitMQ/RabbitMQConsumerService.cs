using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;

namespace Rent_Motorcycle.Services.RabbitMQ
{
    public class RabbitMQConsumerService
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private const string QueueName = "nova-locacao";

        public RabbitMQConsumerService(IConnection connection)
        {
            _connection = connection;
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(queue: QueueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
        }

        public void StartConsuming()
        {
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                // Process the message
                Console.WriteLine($"Received message: {message}");
            };

            _channel.BasicConsume(queue: QueueName, autoAck: true, consumer: consumer);
        }
    }
}
