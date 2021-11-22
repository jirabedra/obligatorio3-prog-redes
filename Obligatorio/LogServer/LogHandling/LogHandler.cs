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
            var factory = new ConnectionFactory() { HostName = "localhost" }; //1 - Defino la conexion
            using (var connection = factory.CreateConnection()) // 2 - Creamos la conexion
            using (var channel = connection.CreateModel()) //3 / Definimos el canal de conexion
            {
                channel.QueueDeclare(queue: "obligatorioPRedes", // 4 - en el canal, definimos la Queue de la conexion
                    durable: false,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                var consumer = new EventingBasicConsumer(channel); // 5 - definimos como consumimos los mensajes
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    newLogs.Add(message);
                };
                channel.BasicConsume(queue: "obligatorioPRedes",
                    autoAck: true,
                    consumer: consumer);
                Console.ReadLine();
            }
            return newLogs;
        }
    }
}
