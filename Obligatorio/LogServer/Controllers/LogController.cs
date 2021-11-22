using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        //Si es 1## es filtrado por Usuario (la id del usuario)
        //Si es 2## es filtrado por Juego (titulo del juego)
        //Si es 1## es filtrado por Fecha (dd/mm/yyyy)
        //https://localhost:44374/logs/1##userId=68,userId=70
        //https://localhost:44374/logs/2##game=Fornite,game=Minecraft
        //https://localhost:44374/logs/3##date=20/10/2021

        [HttpGet("{query}")]
        public async Task<IActionResult> Get(string query)
        {
            if (query == null)
            {
                return BadRequest();
            }
            else
            {
                try 
                {
                    int filterReq = Int32.Parse(query.Split("##")[0]);
                    string filterData = query.Split("##")[1];
                    string ret = FilterLogs(filterReq, filterData);
                    return Ok(ret);
                }
                catch (Exception e) 
                { 
                    return BadRequest(); 
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
                }
                throw new ArgumentOutOfRangeException();
            }
            catch (Exception e) 
            {
                throw new Exception();
            }
        }

        private string FilterByDate(string filterData)
        {
            throw new NotImplementedException();
        }

        private string FilterByGame(string filterData)
        {
            throw new NotImplementedException();
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
            List<Log> logsfiltered = new List<Log>(); //Aca hago la llamada a LogService
            ret = JsonConvert.SerializeObject(logsfiltered);
            return ret;
        }
    }
}
