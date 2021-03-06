using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using LogServer.LogServices.Implementations;
using LogServer.LogServices.Interfaces;
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
        //Si filter es 1 es filtrado por Usuario (la id del usuario)
        //Si filter es 2 es filtrado por Juego (titulo del juego)
        //Si filter es 3 es filtrado por Fecha (yyyy/MM/dd)
        //Si filter es 4 trae todos los logs
        //https://localhost:44374/logs?filter=1&userId=68,userId=70
        //https://localhost:44374/logs?filter=2&game=Fornite,game=Minecraft
        //https://localhost:44374/logs?filter=3&date=2021-10-20 (yyyy-MM-dd)
        //https://localhost:44374/logs?filter=4

        [HttpGet]
        public IActionResult Get()
        {
            string query = Request.QueryString.ToString();
            var parsed = HttpUtility.ParseQueryString(query);
            if (query == null)
            {
                return BadRequest();
            }
            else
            {
                try
                {
                    int filterReq = Int32.Parse(parsed["filter"]);
                    string ret = FilterLogs(filterReq, query);
                    return Ok(ret);
                }
                catch (Exception e)
                {
                    return BadRequest(e.Message);
                }
            }
        }

        private string FilterLogs(int filterReq, string query)
        {
            try
            {
                switch (filterReq)
                {
                    case 1:
                        return FilterByUser(query);
                    case 2:
                        return FilterByGame(query);
                    case 3:
                        return FilterByDate(query);
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

        private string FilterByDate(string query)
        {
            var parsed = HttpUtility.ParseQueryString(query);
            string filterData = parsed["date"];
            string ret = "";
            DateTime dateToFilter = DateTime.Parse(filterData);
            List<Log> logsfiltered = _logService.FilterByDate(dateToFilter);
            ret = JsonConvert.SerializeObject(logsfiltered);
            return ret;
        }

        private string FilterByGame(string query)
        {
            var parsed = HttpUtility.ParseQueryString(query);
            string filterData = parsed["game"];
            string ret = "";
            string[] usersSplited = filterData.Split(",");
            List<string> gamesToFilter = new List<string>();
            foreach (var userToFilter in usersSplited)
            {
                gamesToFilter.Add(userToFilter);
            }
            List<Log> logsfiltered = _logService.FilterByGame(gamesToFilter);
            ret = JsonConvert.SerializeObject(logsfiltered);
            return ret;
        }

        private string FilterByUser(string query)
        {
            var parsed = HttpUtility.ParseQueryString(query);
            string filterData = parsed["userId"];
            string ret = "";
            string[] usersSplited = filterData.Split(",");
            List<int> usersToFilter = new List<int>();
            foreach (var userToFilter in usersSplited)
            {
                usersToFilter.Add(Int32.Parse(userToFilter));
            }
            List<Log> logsfiltered = _logService.FilterByUser(usersToFilter);
            ret = JsonConvert.SerializeObject(logsfiltered);
            return ret;
        }
    }
}
