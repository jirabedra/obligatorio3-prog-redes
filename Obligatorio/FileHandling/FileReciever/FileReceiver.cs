using CommonFileSenderReceiver.FileHandler;
using CommonFileSenderReceiver.FileHandler.Interfaces;
using CommonFileSenderReceiver.NetworkUtils;
using CommonFileSenderReceiver.NetworkUtils.Interfaces;
using CommonFileSenderReceiver.Protocol;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace FileHandling.FileReceiver
{
    public class FileReceiver
    {
        private readonly TcpClient _tcpClient;
        private readonly IFileStreamHandler _fileStreamHandler;
        private INetworkStreamHandler _networkStreamHandler;

        public FileReceiver()
        {
            _tcpClient = new TcpClient(new IPEndPoint(IPAddress.Parse(ConfigurationManager.AppSettings.Get("ClientIpAddress")), Int32.Parse(ConfigurationManager.AppSettings.Get("ClientPort"))));
            _fileStreamHandler = new FileStreamHandler();
        }

        public void StartClient()
        {
            _tcpClient.Connect(IPAddress.Parse(ConfigurationManager.AppSettings.Get("ServerIpAddress")), Int32.Parse(ConfigurationManager.AppSettings.Get("ServerPort")));
            _networkStreamHandler = new NetworkStreamHandler(_tcpClient.GetStream());
        }

        public void ReceiveFile()
        {
            // 1) Recibo 12 bytes
            // 2) Tomo los 4 primeros bytes para saber el largo del nombre del archivo
            // 3) Tomo los siguientes 8 bytes para saber el tamaño del archivo
            var header = _networkStreamHandler.Read(Header.GetLength());
            var fileNameSize = BitConverter.ToInt32(header, 0);
            var fileSize = BitConverter.ToInt64(header, Specification.FixedFileNameLength);

            // 4) Recibo el nombre del archivo
            var fileName = Encoding.UTF8.GetString(_networkStreamHandler.Read(fileNameSize));

            // 5) Calculo la cantidad de partes a recibir
            long parts = SpecificationHelper.GetParts(fileSize);
            long offset = 0;
            long currentPart = 1;

            Console.WriteLine($"Voy a recibir un archivo de tamaño {fileSize} en {parts} partes");

            while (fileSize > offset)
            {
                byte[] data;
                if (currentPart == parts)
                {
                    var lastPartSize = (int)(fileSize - offset);
                    Console.WriteLine($"Recibi un segmento de tamaño {lastPartSize}");
                    data = _networkStreamHandler.Read(lastPartSize);
                    offset += lastPartSize;
                }
                else
                {
                    Console.WriteLine($"Recibi un segmento de tamaño {Specification.MaxPacketSize}");
                    data = _networkStreamHandler.Read(Specification.MaxPacketSize);
                    offset += Specification.MaxPacketSize;
                }
                fileName = ConfigurationManager.AppSettings.Get("ReceivedDataPath") + fileName;
                _fileStreamHandler.Write(fileName, data);
                currentPart++;
            }
        }
    }
}
