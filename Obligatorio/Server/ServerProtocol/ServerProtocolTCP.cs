using BusinessLogic;
using DataAccess.Repositories;
using Grpc.Core;
using LogsLogic;
using ProtocolLibrary;
using RabbitMQ.Client;
using Server.Protos;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServerProtocol.Protocol
{
    public class ServerProtocolTCP : UserService.UserServiceBase
    {
        private bool _exit = false;
        private readonly TcpListener _tcpListener = new TcpListener((new IPEndPoint(IPAddress.Parse(ConfigurationManager.AppSettings.Get("ServerIpAddress")),
                            Int32.Parse(ConfigurationManager.AppSettings.Get("NewServerPort")))));
        private Semaphore _gameSemaphore = new Semaphore(1, 1);
        private Semaphore _userSemaphore = new Semaphore(1, 1);
        private static GameRepository _gameRepository = new GameRepository();
        private static UserRepository _userRepository = new UserRepository();
        ConnectionFactory logFactory = new ConnectionFactory() { HostName = "localhost" };

        public async Task RunServer()
        {
            //LoadTestData();
            //SendBasicLogTest();
            _tcpListener.Start();
            var listenForTcpClients = ListenForTCPClients();
            Console.WriteLine("The server application is now running.");
            PrintMenu();
            while (!_exit)
            {
                HandleMenu();
            }
        }

        private async Task LoadTestData()
        {
            Game game1 = new Game { Title = "Zelda", Genre = "adventure", Overview = "Save the princes.", Reviews = new List<Review>() };
            _gameRepository.addGame(game1);
            User user1 = new User(new List<Game>());
        }

        private async Task ListenForTCPClients()
        {
            while (!_exit)
            {
                try
                {
                    var incomingClient = await _tcpListener.AcceptTcpClientAsync();
                    Console.WriteLine("Accepted new connection through TCPClient...");
                    var task = Task.Run(async () => HandleClient(incomingClient));
                }
                catch (Exception e)
                {
                    Console.WriteLine("Current client couldn't be handled appropriately.");
                }
            }
            Console.WriteLine("Exiting....");
        }

        private async Task HandleClient(TcpClient incomingClient)
        {
            bool exit = false;
            while (!exit)
            {
                var headerLength = HeaderConstants.Request.Length + HeaderConstants.CommandLength +
                                   HeaderConstants.DataLength;
                var headerBuffer = new byte[headerLength];
                try
                {
                    exit = await ReceiveTCPHeader(incomingClient, headerLength, headerBuffer).ConfigureAwait(false);
                    var header = new Header();
                    header.DecodeData(headerBuffer);
                    if (!exit)
                    {
                        var dataBuffer = new byte[header.IDataLength];
                        switch (header.ICommand)
                        {
                            case CommandConstants.Message:
                                await ReceiveTCPData(incomingClient, header.IDataLength, dataBuffer);
                                break;
                            case CommandConstants.Login:
                                string userIdRegistered = await DoLogin();
                                await SendResponseToClient(userIdRegistered, CommandConstants.Login, incomingClient);
                                break;
                            case CommandConstants.PublishGame:
                                string gameToPublish = await ReceiveStringTCPData(incomingClient, header.IDataLength, dataBuffer);
                                string gamePublished = await ParseAndPublishGame(gameToPublish);
                                if (gamePublished == null)
                                {
                                    await SendResponseToClient(null, CommandConstants.PublishGame, incomingClient);
                                }
                                else
                                {
                                    await SendResponseToClient(gamePublished, CommandConstants.PublishGame, incomingClient);
                                }
                                break;
                            case CommandConstants.EditGame:
                                string gameToEdit = await ReceiveStringTCPData(incomingClient, header.IDataLength, dataBuffer);
                                string gameEdited = await ParseGameToEdit(gameToEdit);
                                if (gameEdited == "")
                                {
                                    await SendResponseToClient(null, CommandConstants.EditGame, incomingClient);
                                }
                                else
                                {
                                    await SendResponseToClient(gameEdited, CommandConstants.EditGame, incomingClient);
                                }
                                break;
                            case CommandConstants.ListGames:
                                string gameList = ListGames();
                                await SendResponseToClient(gameList, CommandConstants.ListGames, incomingClient);
                                break;
                            case CommandConstants.DeleteGame:
                                string gameToDelete = await ReceiveStringTCPData(incomingClient, header.IDataLength, dataBuffer);
                                string gameDeleted = await ParseAndDeleteGame(gameToDelete);
                                if (gameDeleted == "")
                                {
                                    await SendResponseToClient(null, CommandConstants.DeleteGame, incomingClient);
                                }
                                else
                                {
                                    await SendResponseToClient(gameDeleted, CommandConstants.DeleteGame, incomingClient);
                                }
                                break;
                            case CommandConstants.GameDetail:
                                string gameToDetail = await ReceiveStringTCPData(incomingClient, header.IDataLength, dataBuffer);
                                string gameDetailed = await ParseAndDetailGame(gameToDetail);
                                if (gameDetailed == "")
                                {
                                    await SendResponseToClient(null, CommandConstants.GameDetail, incomingClient);
                                }
                                else
                                {
                                    await SendResponseToClient(gameDetailed, CommandConstants.GameDetail, incomingClient);
                                }
                                break;
                            case CommandConstants.RateGame:
                                string gameToRate = await ReceiveStringTCPData(incomingClient, header.IDataLength, dataBuffer);
                                Console.WriteLine();
                                string gameRated = await ParseAndRateGame(gameToRate);
                                if (gameRated == "")
                                {
                                    await SendResponseToClient(null, CommandConstants.RateGame, incomingClient);
                                }
                                else
                                {
                                    await SendResponseToClient(gameRated, CommandConstants.RateGame, incomingClient);
                                }
                                break;
                            case CommandConstants.PurchaseGame:
                                string gameToPurchase = await ReceiveStringTCPData(incomingClient, header.IDataLength, dataBuffer);
                                string gamePurchased = await ParseAndPurchaseGame(gameToPurchase);
                                if (gamePurchased == "")
                                {
                                    await SendResponseToClient(null, CommandConstants.PurchaseGame, incomingClient);
                                }
                                else
                                {
                                    await SendResponseToClient(gamePurchased, CommandConstants.PurchaseGame, incomingClient);
                                }
                                break;
                            case CommandConstants.GameSearch:
                                string gameToSearch = await ReceiveStringTCPData(incomingClient, header.IDataLength, dataBuffer);
                                string gameSearched = await ParseAndSearchGame(gameToSearch);
                                if (gameSearched == null)
                                {
                                    await SendResponseToClient(null, CommandConstants.GameSearch, incomingClient);
                                }
                                else
                                {
                                    await SendResponseToClient(gameSearched, CommandConstants.GameSearch, incomingClient);
                                }
                                break;
                            case CommandConstants.SeePurchasedGames:
                                int userID = Int32.Parse(await ReceiveStringTCPData(incomingClient, header.IDataLength, dataBuffer));
                                string purchasedGames = await GetPurchasedGames(userID);
                                await SendResponseToClient(purchasedGames, CommandConstants.SeePurchasedGames, incomingClient);
                                break;
                            default:
                                Console.WriteLine("default got {0}", header.ICommand);
                                break;
                        }
                    }
                    else
                    {
                        incomingClient.Close();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        private async Task<string> GetPurchasedGames(int userID)
        {
            string games = "";
            _userSemaphore.WaitOne();
            User u = _userRepository.Users.Find(user => user.Equals(new User(userID, null)));
            foreach (var g in u.Games)
            {
                games += g.Title;
                games += '\n';
                Console.WriteLine(games);
            }
            _userSemaphore.Release();
            return games;
        }

        private async Task<string> ParseAndSearchGame(string gameToSearch)
        {
            var gameToBeSearch = gameToSearch.Split("##");
            string gameSearched = await SearchGame(gameToBeSearch);
            return gameSearched;
        }

        private async Task<string> SearchGame(string[] gameToBeSearch)
        {
            int searchCriteria = Int32.Parse(gameToBeSearch[0]);
            string ret = "";
            switch (searchCriteria)
            {
                case 1:
                    return await SearchGamesByTitle(gameToBeSearch[1].ToLower());
                case 2:
                    return await SearchGameByGenre(gameToBeSearch[1].ToLower());
                case 3:
                    return await SearchGameByRating(Int32.Parse(gameToBeSearch[1]));
                default:
                    return ret;
            }
        }

        private async Task<string> SearchGameByRating(int ratingToCompare)
        {
            _gameSemaphore.WaitOne();
            var aux = new List<Game>(_gameRepository.Games);
            _gameSemaphore.Release();
            string ret = "";
            int gamePosition = 0;
            foreach (var g in aux)
            {
                if (Math.Truncate(g.Rating) == ratingToCompare)
                {
                    gamePosition++;
                    ret += $"{gamePosition}. {g.Title} with a rating of {g.Rating.ToString()}. \n";
                }
            }
            return ret;
        }

        private async Task<string> SearchGameByGenre(string genreToCompare)
        {
            _gameSemaphore.WaitOne();
            var aux = new List<Game>(_gameRepository.Games);
            _gameSemaphore.Release();
            int gamePosition = 0;
            string ret = "";
            foreach (var g in aux)
            {
                if (g.Genre.ToLower().Contains(genreToCompare.ToLower()))
                {
                    gamePosition++;
                    ret += $"{gamePosition}. {g.Title}\n";
                }
            }
            return ret;
        }

        private async Task<string> SearchGamesByTitle(string titleToCompare)
        {
            _gameSemaphore.WaitOne();
            var aux = new List<Game>(_gameRepository.Games);
            _gameSemaphore.Release();
            int gamePosition = 0;
            string ret = "";
            foreach (var g in aux)
            {
                if (g.Title.ToLower().Contains(titleToCompare))
                {
                    gamePosition++;
                    ret += $"{gamePosition}. {g.Title}\n";
                }
            }
            return ret;
        }

        private async Task<string> ParseAndPurchaseGame(string gameToPurchase)
        {
            var dataGameToPurchase = gameToPurchase.Split("##");
            return await PurchaseGame(dataGameToPurchase);
        }

        private async Task<string> PurchaseGame(string[] dataGameToPurchase)
        {
            string gameToPurchase = dataGameToPurchase[0];
            _gameSemaphore.WaitOne();
            if (_gameRepository.IsRegistered(gameToPurchase))
            {
                int gameIndex = _gameRepository.GetIndex(gameToPurchase);
                if (gameIndex != -1)
                {
                    _userSemaphore.WaitOne();
                    _userRepository.Users[Int32.Parse(dataGameToPurchase[1]) - 1].AddGame(_gameRepository.Games[gameIndex]);
                    _userSemaphore.Release();
                    _gameSemaphore.Release();
                    return gameToPurchase;
                }
            }
            _gameSemaphore.Release();
            return "";
        }

        private async Task<string> ParseAndRateGame(string gameToRate)
        {
            var dataGameToRate = gameToRate.Split("##");
            return await RateGame(dataGameToRate);
        }

        private async Task<string> RateGame(string[] dataGameToRate)
        {
            string gameToRate = dataGameToRate[0];
            Review r = new Review { Comment = dataGameToRate[1], Rate = Int32.Parse(dataGameToRate[2]) };
            _gameSemaphore.WaitOne();
            if (_gameRepository.IsRegistered(gameToRate))
            {
                int gameIndex = _gameRepository.GetIndex(gameToRate);
                if (gameIndex != -1)
                {
                    _gameRepository.Games[gameIndex].AddReview(r);
                    _gameSemaphore.Release();
                    return dataGameToRate[0];
                }
            }
            _gameSemaphore.Release();
            return "";
        }

        private async Task<string> ParseAndDetailGame(string gameToDetail)
        {
            return await DetailGame(gameToDetail);
        }

        private async Task<string> DetailGame(string gameToDetail)
        {
            string ret = "";
            _gameSemaphore.WaitOne();
            var aux = new List<Game>(_gameRepository.Games);
            _gameSemaphore.Release();
            foreach (var game in aux)
            {
                if (game.Title.Equals(gameToDetail))
                {
                    ret =
                        $"Game title: {game.Title}\n" +
                        $"Game genre: {game.Genre}\n" +
                        $"Game overview: {game.Overview}\n" +
                        $"Game rating: {game.Rating}\n";
                    if (game.Reviews != null)
                    {
                        ret += $"Game reviews: \n{game.GetReviewsToPrint()}";
                    }
                    return ret;
                }
            }
            return ret;
        }

        private async Task<string> ParseAndDeleteGame(string gameToDelete)
        {
            return await DeleteGame(gameToDelete);
        }

        private async Task<string> DeleteGame(string gameToDelete)
        {
            _gameSemaphore.WaitOne();
            if (_gameRepository.IsRegistered(gameToDelete))
            {
                int gameIndex = _gameRepository.GetIndex(gameToDelete);
                if (gameIndex != -1)
                {
                    string gameDeleted = _gameRepository.DeleteGame(gameIndex);
                    _userSemaphore.WaitOne();
                    _userRepository.DeleteGame(gameToDelete);
                    _userSemaphore.Release();
                    _gameSemaphore.Release();
                    if (gameDeleted == null)
                    {
                        return "";
                    }
                    else
                    {
                        return gameDeleted;
                    }
                }
            }
            _gameSemaphore.Release();
            return "";
        }

        private async Task<string> DoLogin()
        {
            User u = new User(new List<Game>());
            _userSemaphore.WaitOne();
            _userRepository.Users.Add(u);
            _userSemaphore.Release();
            return u.Id.ToString();
        }
        private async Task<string> ParseAndPublishGame(string gameToPublish)
        {
            var gameToAdd = gameToPublish.Split("##");
            string nameGameAdded = await AddGame(gameToAdd);
            return nameGameAdded;
        }

        private async Task<string> AddGame(string[] gameToAdd)
        {
            Game newGame = new Game { Title = gameToAdd[0], Genre = gameToAdd[1].ToLower(), Overview = gameToAdd[2], Reviews = new List<Review>() };
            _gameSemaphore.WaitOne();
            bool successfullyAdded = _gameRepository.addGame(newGame);
            _gameSemaphore.Release();
            Console.WriteLine("value is: " + successfullyAdded);
            if (!successfullyAdded)
            {
                return null;
            }
            return newGame.Title;
        }

        private async Task<string> ParseGameToEdit(string gameToEdit)
        {
            var gameToAdd = gameToEdit.Split("##");
            return await ModifyGame(gameToAdd);
        }

        private async Task<string> ModifyGame(string[] gameToAdd)
        {
            string gameToModify = gameToAdd[0];

            _gameSemaphore.WaitOne();
            if (_gameRepository.IsRegistered(gameToModify))
            {
                int gameIndex = _gameRepository.GetIndex(gameToModify);
                if (gameIndex != -1)
                {
                    if (!_gameRepository.IsRegistered(gameToAdd[1]))
                    {
                        _gameRepository.Games[gameIndex].Title = gameToAdd[1];
                        _gameRepository.Games[gameIndex].Genre = gameToAdd[2];
                        _gameRepository.Games[gameIndex].Overview = gameToAdd[3];
                        _gameSemaphore.Release();
                        return gameToAdd[1];
                    }

                }

            }
            _gameSemaphore.Release();
            return "";
        }

        private async Task SendResponseToClient(string message, int login, TcpClient incomingClient)
        {
            Header header;
            if (message == null)
            {
                header = new Header(HeaderConstants.Response, login, 0);
            }
            else
            {
                header = new Header(HeaderConstants.Response, login, message.Length);
            }
            var data = header.GetRequest();
            if (message == null)
            {
                message = "";
            }
            var messageBytes = Encoding.UTF8.GetBytes(message);
            try
            {
                var writeHeader = Task.Run(async () => await incomingClient.GetStream().WriteAsync(data, 0, data.Length));
                await writeHeader;
                var writeData = Task.Run(async () => await incomingClient.GetStream().WriteAsync(messageBytes, 0, messageBytes.Length));
                await writeData;
            }
            catch (Exception ex)
            {
                Console.WriteLine("The message could not be sent. The server could not receive it.");
            }
        }

        private async Task<bool> ReceiveTCPHeader(TcpClient client, int headerLength, byte[] buffer)
        {
            int length = 0;
            while (length < headerLength)
            {
                try
                {
                    var localReceived = await client.GetStream().ReadAsync(buffer, length, headerLength - length);
                    if (localReceived == 0)
                    {
                        return true;
                    }
                    length += localReceived;

                }
                catch (Exception ex)
                {
                    return true;
                }
            }

            return false;
        }

        private async Task<bool> ReceiveTCPData(TcpClient client, int dataLength, byte[] buffer)
        {
            buffer = new byte[dataLength];
            int length = 0;
            while (length < dataLength)
            {
                try
                {
                    var localReceived = await client.GetStream().ReadAsync(buffer, length, dataLength - length);
                    if (localReceived == 0)
                    {
                        return false;
                    }
                    length += localReceived;

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return false;
                }
            }
            Console.WriteLine(Encoding.UTF8.GetString(buffer));
            return true;
        }

        private async Task<string> ReceiveStringTCPData(TcpClient client, int dataLength, byte[] buffer)
        {
            buffer = new byte[dataLength];
            int length = 0;
            while (length < dataLength)
            {
                try
                {
                    var localReceived = await client.GetStream().ReadAsync(buffer, length, dataLength - length);
                    if (localReceived == 0)
                    {
                        return null;
                    }
                    length += localReceived;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return null;
                }
            }
            return Encoding.UTF8.GetString(buffer);
        }

        private async Task HandleMenu()
        {
            var userInput = Console.ReadLine();
            switch (userInput)
            {
                case "exit":
                    break;
                case "list games":
                    break;
                case "buy game":
                    break;
                case "publish game":
                    break;
                case "publish review":
                    break;
                case "search for game":
                    break;
                case "game details":
                    break;
                case "list users":
                    ListAllUsers();
                    break;
                default:
                    Console.WriteLine("Invalid option.");
                    break;
            }
        }

        private async Task PrintMenu()
        {
            Console.WriteLine("-------------------");
            Console.WriteLine("Available options: ");
            Console.WriteLine("exit -> shut downs the application");
            Console.WriteLine("list games -> will list all registered games");
            Console.WriteLine("buy game");
            Console.WriteLine("publish review");
            Console.WriteLine("search for game");
            Console.WriteLine("game details");
            Console.WriteLine("list users");
            Console.WriteLine("Enter the desired option: ");
        }

        private string ListGames()
        {
            string ret = "";
            _gameSemaphore.WaitOne();
            var aux = new List<Game>(_gameRepository.Games);
            _gameSemaphore.Release();
            int cont = 1;
            foreach (var game in aux)
            {
                ret += $"{cont++}. {game.Title}\n";
            }
            return ret;
        }

        public override Task<Response> AddUser(UserProto request, ServerCallContext context)
        {
            User user = new User(new List<Game>());
            _userSemaphore.WaitOne();
            Console.WriteLine($"Users added: {_userRepository.Users.Count}");
            _userRepository.Users.Add(user);
            Console.WriteLine($"Users added: {_userRepository.Users.Count}");
            _userSemaphore.Release();
            return Task.FromResult(new Response()
            {
                Result = true
            });
        }

        public override Task<Response> DeleteUser(UserName id, ServerCallContext context)
        {
            var user = _userRepository.Users.Find(x => x.Id.Equals(id.Name));
            if (user is null)
            {
                return Task.FromResult(new Response()
                {
                    Result = false
                });
            }
            else
            {
                _userRepository.DeleteUser(user);
                return Task.FromResult(new Response()
                {
                    Result = true
                });
            }
        }

        public override Task<Response> UpdateUser(UserUpdate userUpdate, ServerCallContext context)
        {
            var user = _userRepository.Users.Find(x => x.Id.Equals(userUpdate.Name));
            if (user is null)
            {
                return Task.FromResult(new Response()
                {
                    Result = false
                });
            }
            else
            {
                user.Nickname = userUpdate.Nickname;
                return Task.FromResult(new Response()
                {
                    Result = true
                });
            }
        }

        private void ListAllUsers()
        {
            _userSemaphore.WaitOne();
            _userRepository.listAllUsers();
            _userSemaphore.Release();
        }

        private void SendBasicLogTest()
        {
            ConnectionFactory factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "hello",
                    durable: false,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                var message = MessageLog(channel);
                Console.WriteLine(" [x] Sent {0}", message);
            }
        }

        private static string MessageLog(IModel channel)
        {
            string message = "aaaaaaaaaaaaaaaaaaaaaaa"; ;
            Console.WriteLine($"Mensaje ={message}");
            var body = Encoding.UTF8.GetBytes(message);

            channel.BasicPublish(exchange: "",
                routingKey: "hello",
                basicProperties: null,
                body: body);
            return message;
        }

    }
}



