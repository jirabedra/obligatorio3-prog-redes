using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LogServer.LogHandling
{
    public class LogHandler
    {
        public LogHandler() 
        { 
        }

        public List<string> UpdateLogs()
        {
            List<string> newLogs = new List<string>();
            var factory = new ConnectionFactory() { HostName = "localhost" };
            var connection = factory.CreateConnection();
            var channel = connection.CreateModel();
            channel.QueueDeclare(queue: "obligatorioPRedes",
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                newLogs.Add(message);
            };
            channel.BasicConsume(queue: "obligatorioPRedes",
                autoAck: true,
                consumer: consumer);
            Thread.Sleep(1000);
            return newLogs;
        }
    }
}
