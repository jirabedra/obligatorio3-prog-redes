using ProtocolLibrary;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleClientProtocol.Protocol
{
    public class ClientProtocolTCP
    {
        private bool _exit = false;
        private readonly TcpClient _tcpClient = new TcpClient(new IPEndPoint(IPAddress.Parse(ConfigurationManager.AppSettings.Get("ClientIpAddress")),
                            Int32.Parse(ConfigurationManager.AppSettings.Get("ClientPort"))));
        private int _userId = -1;

        public async Task ConnectToServer()
        {
            var task = Task.Run(async () => await EstablishTCPConnection());
            var connected = true;
            Console.WriteLine("Connection to the server has now been established.");
            PrintMenu();
            while (connected)
            {
                var option = Console.ReadLine();
                switch (option)
                {
                    case "exit":
                        connected = false;
                        _tcpClient.Close();
                        break;
                    case "message":
                        await SendMessageToServer();
                        break;
                    case "login":
                        await RequestLogin();
                        Console.WriteLine($"Your user id is {_userId}");
                        break;
                    case "new game":
                        if (_userId != -1)
                            await RequestNewGame();
                        else
                            Console.WriteLine("Need to be logged in to use this option.");
                        break;
                    case "edit game":
                        if (_userId != -1)
                            await RequestEditGame();
                        else
                            Console.WriteLine("Need to be logged in to use this option.");
                        break;
                    case "delete game":
                        if (_userId != -1)
                            await RequestDeleteGame();
                        else
                            Console.WriteLine("Need to be logged in to use this option.");
                        break;
                    case "search game":
                        await RequestSearchGame();
                        break;
                    case "rate game":
                        if (_userId != -1)
                            await RequestRateGame();
                        else
                            Console.WriteLine("Need to be logged in to use this option.");
                        break;
                    case "game summary":
                        await RequestDetailsGame();
                        break;
                    case "list games":
                        await RequestGameList();
                        break;
                    case "purchase game":
                        if (_userId != -1)
                            await RequestPurchaseGame();
                        else
                            Console.WriteLine("Need to be logged in to use this option.");
                        break;
                    case "see purchased games":
                        if (_userId != -1)
                            await RequestSeePurchasedGames();
                        else
                            Console.WriteLine("Need to be logged in to use this option.");
                        break;
                    default:
                        Console.WriteLine("Invalid option");
                        break;
                }
                PrintMenu();
            }
        }

        private async Task RequestSeePurchasedGames()
        {
            try
            {
                Console.WriteLine("Requesting your purchased games.");
                Task purchased = Task.Run(async () => await SendRequestToServer(CommandConstants.SeePurchasedGames, _userId.ToString()));
                await purchased;
                string gotResponse = await ReceiveServerResponse(CommandConstants.SeePurchasedGames);
                if (gotResponse == "")
                {
                    Console.WriteLine("You haven't purchased any games yet.");
                }
                else
                {
                    Console.WriteLine($"The games you have purchased are: \n{gotResponse}.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("The purchased games could not be retrieved. Please try again.");
            }
        }

        private async Task RequestSearchGame()
        {
            try
            {
                string data = await AskForGameToBeSearchedData();
                Console.WriteLine("Requesting to seach a game to the server");
                Task newGame = Task.Run(async () => await SendRequestToServer(CommandConstants.GameSearch, data));
                await newGame;
                string gotResponse = await ReceiveServerResponse(CommandConstants.GameSearch);
                if (gotResponse == "")
                {
                    Console.WriteLine("No found games.");
                }
                else
                {
                    Console.WriteLine($"The game: \n{gotResponse}.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("The search of the game was unsuccessful.");
            }
        }

        private async Task<string> AskForGameToBeSearchedData()
        {
            int searchCriteria = 0;
            string ret = "";
            string searchData = "";
            bool validSearchData = false;
            while (!validSearchData)
            {
                searchCriteria = 0;
                ret = "";
                try
                {
                    Console.WriteLine("Please choose a seach criteria: \n1 - title \n2 - genre \n3 - rating \n");
                    searchCriteria = Int32.Parse(Console.ReadLine().Trim());
                    while (searchCriteria < 1 || searchCriteria > 3)
                    {
                        Console.WriteLine("Wrong number.\nEnter a valid number (1-3).\n");
                        searchCriteria = Int32.Parse(Console.ReadLine().Trim());
                    }
                    searchData = await AskForGameSearchCriteria(searchCriteria);
                    ret = searchCriteria.ToString() + "##" + searchData;
                    validSearchData = true;
                }
                catch (FormatException e)
                {
                    Console.WriteLine("Number required.");
                }
                catch (ArgumentOutOfRangeException e2)
                {
                    Console.WriteLine("Rating ranges from 1 to 5.");
                }
                catch (ArgumentNullException e3)
                {
                    Console.WriteLine("Please enter something.");
                }
                catch (Exception e4)
                {
                    Console.WriteLine("Something unexpected happened, please try again.");
                }
            }
            return ret;
        }

        private async Task<string> AskForGameSearchCriteria(int searchCriteria)
        {
            string dataToReturn = "";
            switch (searchCriteria)
            {
                case 1:
                    Console.WriteLine("Enter a title or a part of it.");
                    dataToReturn = Console.ReadLine().Trim();
                    if (dataToReturn == "")
                    {
                        throw new ArgumentNullException();
                    }
                    return dataToReturn;
                case 2:
                    Console.WriteLine("Enter a genre or a part of it.");
                    dataToReturn = Console.ReadLine().Trim();
                    if (dataToReturn == "")
                    {
                        throw new ArgumentNullException();
                    }
                    return dataToReturn;
                case 3:
                    Console.WriteLine("Enter a rating between 1 and 5.");
                    dataToReturn = Console.ReadLine();
                    if (Int32.Parse(dataToReturn.Trim()) > 0 && Int32.Parse(dataToReturn.Trim()) < 6)
                    {
                        return dataToReturn;
                    }
                    else
                    {
                        throw new ArgumentOutOfRangeException();
                    }
            }
            return "";
        }

        private async Task RequestPurchaseGame()
        {
            try
            {
                string data = AskForGameTitleToPurchase();
                Console.WriteLine("Requesting to purchase a game to the server.");
                Console.WriteLine(data);
                data += "##" + _userId.ToString();
                Task deleteGame = Task.Run(async () => await SendRequestToServer(CommandConstants.PurchaseGame, data));
                await deleteGame;
                string gotResponse = await ReceiveServerResponse(CommandConstants.PurchaseGame);
                if (gotResponse == "")
                {
                    Console.WriteLine("Request failed: the name of the game does not exists.");
                }
                else
                {
                    Console.WriteLine($"The game named: {gotResponse}, has been successfully Purchased.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unexpected error.");
            }
        }

        private string AskForGameTitleToPurchase()
        {
            string s = "";
            do
            {
                Console.WriteLine("Enter the name of the game to be purchase:");
                s = Console.ReadLine();
            } while (s.Length == 0);
            return s;
        }

        private async Task RequestRateGame()
        {
            try
            {
                string data = AskForGameTitleToReview();
                data += "##" + await AskForReviewGameData();
                Console.WriteLine("Requesting to rate a game to the server");
                Console.WriteLine(data);
                Task editGame = Task.Run(async () => await SendRequestToServer(CommandConstants.RateGame, data));
                await editGame;
                string gotResponse = await ReceiveServerResponse(CommandConstants.RateGame);
                if (gotResponse == "")
                {
                    Console.WriteLine("Request failed: the name of the game does not exist.");
                }
                else
                {
                    Console.WriteLine($"The game named: {gotResponse}, has been successfully rated.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("The creation of the game was unsuccessful.");
            }
        }

        private async Task<string> AskForReviewGameData()
        {
            var gameOpinion = "";
            var gameRate = "";
            Console.WriteLine("You can not leave empty data.");
            do
            {
                if (gameOpinion.Trim().Length == 0)
                {
                    Console.WriteLine("Make a comment about this game.");
                    gameOpinion = Console.ReadLine();
                }
                if (gameRate.Trim().Length == 0 || gameRate.Trim().Length > 1)
                {
                    Console.WriteLine("Please rate this game (1-5).");
                    gameRate = Console.ReadLine();
                    try
                    {
                        if (Int32.Parse(gameRate.Trim()) < 1 || Int32.Parse(gameRate.Trim()) > 5)
                        {
                            gameRate = "";
                        }
                    }
                    catch (Exception excep)
                    {
                        gameRate = "";
                    }
                }
            }
            while (gameOpinion.Trim().Length == 0 || gameRate.Trim().Length == 0);

            string gameToRate = gameOpinion.Trim() + "##" + gameRate.Trim();

            return gameToRate;
        }

        private string AskForGameTitleToReview()
        {
            string s = "";
            do
            {
                Console.WriteLine("Enter the name of the game to be reviewed:");
                s = Console.ReadLine();
            } while (s.Length == 0);
            return s;
        }

        private async Task RequestDetailsGame()
        {
            try
            {
                string data = AskForGameTitleToGetDetails();
                Console.WriteLine("Requesting to detaile a game to the server");
                Console.WriteLine(data);
                Task detailedGame = Task.Run(async () => await SendRequestToServer(CommandConstants.GameDetail, data));
                await detailedGame;
                string gotResponse = await ReceiveServerResponse(CommandConstants.GameDetail);
                if (gotResponse == "")
                {
                    Console.WriteLine("Request failed: the name of the game does not exists");
                }
                else
                {
                    Console.WriteLine($"The game details requested are:\n {gotResponse}.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("The search of the game was unsuccessful.");
            }
        }

        private string AskForGameTitleToGetDetails()
        {
            string s = "";
            do
            {
                Console.WriteLine("Enter the name of the game to be detailed:");
                s = Console.ReadLine();
            } while (s.Length == 0);
            return s;
        }

        private async Task RequestDeleteGame()
        {
            try
            {
                string data = AskForGameTitleToDelete();
                Console.WriteLine("Requesting to delte a game to the server");
                Console.WriteLine(data);
                Task deleteGame = Task.Run(async () => await SendRequestToServer(CommandConstants.DeleteGame, data));
                await deleteGame;
                string gotResponse = await ReceiveServerResponse(CommandConstants.DeleteGame);
                if (gotResponse == "")
                {
                    Console.WriteLine("Request failed: the name of the game does not exists");
                }
                else
                {
                    Console.WriteLine($"The game named: {gotResponse}, has been successfully deleted.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("The deletion of the game was unsuccessful.");
            }
        }

        private async Task RequestGameList()
        {
            try
            {
                Console.WriteLine("Requesting all the games registered.");
                Task list = Task.Run(async () => await SendRequestToServer(CommandConstants.ListGames));
                await list;
                string games = await ReceiveServerResponse(CommandConstants.ListGames);
                Console.WriteLine("--------------GAMES--------------");
                Console.WriteLine(games);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Games list could not be retrieved.");
            }
        }

        private async Task RequestLogin()
        {
            try
            {
                Console.WriteLine("Requesting login to server");
                Task login = Task.Run(async () => await SendRequestToServer(CommandConstants.Login));
                await login;
                _userId = Int32.Parse(await ReceiveServerResponse(CommandConstants.Login));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Login request unsuccessful.");
            }

        }

        private async Task<string> ReceiveServerResponse(int cc)
        {
            try
            {
                switch (cc)
                {
                    case 1:
                        string id = await ListenForResponse();
                        return id;
                    case 5:
                        return await ListenForResponse();
                    case 6:
                        return await ListenForResponse();
                    case 7:
                        return await ListenForResponse();
                    case 8:
                        return await ListenForResponse();
                    case 9:
                        return await ListenForResponse();
                    case 10:
                        return await ListenForResponse();
                    case 13:
                        return await ListenForResponse();
                    case 14:
                        return await ListenForResponse();
                    case 15:
                        return await ListenForResponse();
                        break;
                    default:
                        throw new Exception("Unhandled response header received.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }

        }

        private async Task<string> ListenForResponse()
        {
            var headerLength = HeaderConstants.Response.Length + HeaderConstants.CommandLength +
                   HeaderConstants.DataLength;
            byte[] headerBuffer = new byte[headerLength];
            var received = await ReceiveTCPHeader(headerLength, headerBuffer);
            if (!received)
            {
                var header = new Header();
                header.DecodeData(headerBuffer);
                byte[] dataBuffer = new byte[header.IDataLength];
                switch (header.ICommand)
                {
                    case 1:
                        return await ReceiveTCPData(header.IDataLength, dataBuffer);
                    case 5:
                        return await ReceiveTCPData(header.IDataLength, dataBuffer);
                    case 6:
                        return await ReceiveTCPData(header.IDataLength, dataBuffer);
                    case 7:
                        return await ReceiveTCPData(header.IDataLength, dataBuffer);
                    case 8:
                        return await ReceiveTCPData(header.IDataLength, dataBuffer);
                    case 9:
                        return await ReceiveTCPData(header.IDataLength, dataBuffer);
                    case 10:
                        return await ReceiveTCPData(header.IDataLength, dataBuffer);
                    case 13:
                        return await ReceiveTCPData(header.IDataLength, dataBuffer);
                    case 14:
                        return await ReceiveTCPData(header.IDataLength, dataBuffer);
                    case 15:
                        return await ReceiveTCPData(header.IDataLength, dataBuffer);
                }
            }
            else
            {
                return null;
            }
            return null;
        }

        private async Task RequestNewGame()
        {
            try
            {
                string data = await AskForNewGameData();
                Console.WriteLine("Requesting to add a new game to the server");
                Task newGame = Task.Run(async () => await SendRequestToServer(CommandConstants.PublishGame, data));
                await newGame;
                string gotResponse = await ReceiveServerResponse(CommandConstants.PublishGame);
                if (gotResponse == "")
                {
                    Console.WriteLine("Request failed: the name of the game already exists");
                }
                else
                {
                    Console.WriteLine($"The game named: {gotResponse}, has been successfully added.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("The creation of the game was unsuccessful.");
            }
        }

        private async Task<string> AskForNewGameData()
        {
            var title = "";
            var genre = "";
            var overview = "";
            Console.WriteLine("You can not leave empty data.");
            do
            {
                if (title.Trim().Length == 0)
                {
                    Console.WriteLine("Enter: Game Title.");
                    title = Console.ReadLine();
                }
                if (genre.Trim().Length == 0)
                {
                    Console.WriteLine("Enter:Game Genre.");
                    genre = Console.ReadLine();
                }
                if (overview.Trim().Length == 0)
                {
                    Console.WriteLine("Enter: Game Overview.");
                    overview = Console.ReadLine();
                }
            }
            while (title.Trim().Length == 0 || genre.Trim().Length == 0 || overview.Trim().Length == 0);

            string gameToPublish = title.Trim() + "##" + genre.Trim() + "##" + overview.Trim();

            return gameToPublish;
        }

        private async Task RequestEditGame()
        {
            try
            {
                string data = AskForGameTitleToEdit();
                data += "##" + await AskForNewGameData();
                Console.WriteLine("Requesting to modify a game to the server");
                Console.WriteLine(data);
                Task editGame = Task.Run(async () => await SendRequestToServer(CommandConstants.EditGame, data));
                await editGame;
                string gotResponse = await ReceiveServerResponse(CommandConstants.EditGame);
                if (gotResponse == "")
                {
                    Console.WriteLine("Request failed: the name of the game already exists");
                }
                else
                {
                    Console.WriteLine($"The game named: {gotResponse}, has been successfully added.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("The edition of the game was unsuccessful.");
            }
        }

        private string AskForGameTitleToEdit()
        {
            string s = "";
            do
            {
                Console.WriteLine("Enter the game to be modified's name:");
                s = Console.ReadLine();
            } while (s.Length == 0);
            return s;
        }

        private string AskForGameTitleToDelete()
        {
            string s = "";
            do
            {
                Console.WriteLine("Enter the name of the game to be deleted:");
                s = Console.ReadLine();
            } while (s.Length == 0);
            return s;
        }

        private async Task<bool> ReceiveTCPHeader(int headerLength, byte[] buffer)
        {
            int length = 0;
            while (length < headerLength)
            {
                try
                {
                    var localReceived = await _tcpClient.GetStream().ReadAsync(buffer, length, headerLength - length);
                    if (localReceived == 0)
                    {

                        return true;
                    }
                    length += localReceived;

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            return false;
        }

        private async Task<string> ReceiveTCPData(int dataLength, byte[] buffer)
        {

            int length = 0;
            while (length < dataLength)
            {
                try
                {
                    var localReceived = await _tcpClient.GetStream().ReadAsync(buffer, length, dataLength - length);
                    if (localReceived == 0)
                    {
                        return null;
                    }
                    length += localReceived;

                }
                catch (Exception ex)
                {
                    Console.WriteLine("Something unexpected happened.");
                }
            }
            return Encoding.UTF8.GetString(buffer);
        }

        private async Task SendRequestToServer(int cc)
        {
            var header = new Header(HeaderConstants.Request, cc, 0);
            var headerData = header.GetRequest();
            try
            {
                var writeHeader = Task.Run(async () => await _tcpClient.GetStream().WriteAsync(headerData, 0, headerData.Length));
                await writeHeader;
            }
            catch (Exception ex)
            {
                Console.WriteLine("The message could not be sent. The server could not receive it.");
            }

        }

        private async Task SendRequestToServer(int cc, String data)
        {
            var header = new Header(HeaderConstants.Request, cc, data.Length);
            var headerData = header.GetRequest();
            var messageBytes = Encoding.UTF8.GetBytes(data);
            try
            {
                var writeHeader = Task.Run(async () => await _tcpClient.GetStream().WriteAsync(headerData, 0, headerData.Length));
                await writeHeader;
                var writeData = Task.Run(async () => await _tcpClient.GetStream().WriteAsync(messageBytes, 0, messageBytes.Length));
                await writeData;
            }
            catch (Exception ex)
            {
                Console.WriteLine("The message could not be sent. The server could not receive it.");
            }

        }

        private async Task SendMessageToServer()
        {
            Console.WriteLine("Enter the message to be sent to the server:");
            var message = Console.ReadLine();
            var header = new Header(HeaderConstants.Request, CommandConstants.Message, message.Length);
            var data = header.GetRequest();
            var messageBytes = Encoding.UTF8.GetBytes(message);
            try
            {
                Console.WriteLine("Will send >> " + Encoding.UTF8.GetString(data));
                var writeHeader = Task.Run(async () => await _tcpClient.GetStream().WriteAsync(data, 0, data.Length));
                await writeHeader;
                Console.WriteLine("Will send >> " + Encoding.UTF8.GetString(messageBytes));
                var writeData = Task.Run(async () => await _tcpClient.GetStream().WriteAsync(messageBytes, 0, messageBytes.Length));
                await writeData;
                Console.WriteLine("The following message has been sent to the server >> {0}", message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("The message could not be sent. The server could not receive it.");
            }
        }

        private async Task EstablishTCPConnection()
        {
            _tcpClient.Connect(new IPEndPoint(IPAddress.Parse(ConfigurationManager.AppSettings.Get("NewServerIpAddress")),
                           Int32.Parse(ConfigurationManager.AppSettings.Get("NewServerPort"))));
            var serverThreadTCP = new Thread(() => HandleServer());
            serverThreadTCP.Start();
        }

        private void HandleServer()
        {
            while (!_exit)
            {
                var headerLength = HeaderConstants.Response.Length + HeaderConstants.CommandLength +
                                   HeaderConstants.DataLength;
                var buffer = new byte[headerLength];
                try
                {

                }
                catch (Exception ex)
                {

                }
            }
        }

        private static void PrintMenu()
        {
            Console.WriteLine("-------------------");
            Console.WriteLine("Available options: ");
            Console.WriteLine("login");
            Console.WriteLine("message -> send a message to the server");
            Console.WriteLine("new game -> registers a new game");
            Console.WriteLine("edit game -> change the title, genre or overview");
            Console.WriteLine("delete game");
            Console.WriteLine("search game");
            Console.WriteLine("rate game");
            Console.WriteLine("game summary");
            Console.WriteLine("list games");
            Console.WriteLine("purchase game");
            Console.WriteLine("see purchased games");
            Console.WriteLine("exit -> shuts down the aplication");
            Console.WriteLine("Enter your desired option: ");
        }
    }
}
