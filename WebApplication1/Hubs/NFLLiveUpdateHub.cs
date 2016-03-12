using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;

//The method can execute synchronously and return IEnumerable<Stock> because it is returning data from memory. 
//If the method had to get the data by doing something that would involve waiting, 
//such as a database lookup or a web service call, 
//you would specify Task<IEnumerable<Stock>> as the return value to enable asynchronous processing.
namespace WebApplication1.Hubs
{
    public class NFLLiveUpdateHub : Hub
    {
        private readonly NFLStatsUpdate _statsUpdate;

        public NFLLiveUpdateHub() : this(NFLStatsUpdate.Instance) { }

        public NFLLiveUpdateHub(NFLStatsUpdate nflStatsUpdate)
        {
            _statsUpdate = nflStatsUpdate;
        }

        public IEnumerable<NFLPlayer> GetAllPlayers()
        {
            return _statsUpdate.GetAllPlayers();
        }
    }
}