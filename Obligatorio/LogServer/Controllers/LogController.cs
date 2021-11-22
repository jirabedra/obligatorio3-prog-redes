using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LogServer.LogServices.Implementations;
using LogServer.LogServices.Interfaces;
using LogsLogic;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace LogServer.Controllers
{
    [EnableCors("MyPolicy")]
    [Route("logs")]
    [ApiController]
    public class LogController : Controller
    {
        private static ILogService _logService = new LogService();
        //Si es 1## es filtrado por Usuario (la id del usuario)
        //Si es 2## es filtrado por Juego (titulo del juego)
        //Si es 3## es filtrado por Fecha (dd/mm/yyyy)
        //Si es 4## trae todos los logs
        //https://localhost:44374/logs/1%%userId=68,userId=70
        //https://localhost:44374/logs/2%%game=Fornite,game=Minecraft
        //https://localhost:44374/logs/3%%date=20/10/2021
        //https://localhost:44374/logs/4%%

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            //query = HttpContext.Request.Query["query"];
            string query = HttpContext.Request.Query["name"];
            if (query == null)
            {
                return Ok();
            }
            else
            {
                try 
                {
                    int filterReq = Int32.Parse(query.Split("%%")[0]);
                    string filterData = query.Split("%%")[1];
                    string ret = FilterLogs(filterReq, filterData);
                    return Ok(ret);
                }
                catch (Exception e) 
                { 
                    return Ok(); 
                }
            }
        }

        private string FilterLogs(int filterReq, string filterData)
        {
            try
            {
                switch (filterReq)
                {
                    case 1:
                        return FilterByUser(filterData);
                    case 2:
                        return FilterByGame(filterData);
                    case 3:
                        return FilterByDate(filterData);
                    case 4:
                        return GetAllLogs();
                }
                throw new ArgumentOutOfRangeException();
            }
            catch (Exception e) 
            {
                throw new Exception();
            }
        }

        private string GetAllLogs()
        {
            string ret = "";
            List<Log> logsfiltered = _logService.GetAllLogs();
            ret = JsonConvert.SerializeObject(logsfiltered);
            return ret;
        }

        private string FilterByDate(string filterData)
        {
            throw new NotImplementedException();
        }

        private string FilterByGame(string filterData)
        {
            string ret = "";
            string[] usersSplited = filterData.Split(",");
            List<string> gamesToFilter = new List<string>();
            foreach (var userToFilter in usersSplited)
            {
                gamesToFilter.Add(userToFilter.Split("=")[1]);
            }
            List<Log> logsfiltered = _logService.FilterByGame(gamesToFilter);
            ret = JsonConvert.SerializeObject(logsfiltered);
            return ret;
        }

        private string FilterByUser(string filterData)
        {
            string ret = "";
            string[] usersSplited = filterData.Split(",");
            List<int> usersToFilter = new List<int>();
            foreach (var userToFilter in usersSplited)
            {
                usersToFilter.Add(Int32.Parse(userToFilter.Split("=")[1]));
            }
            List<Log> logsfiltered = _logService.FilterByUser(usersToFilter);
            ret = JsonConvert.SerializeObject(logsfiltered);
            return ret;
        }
    }
}
