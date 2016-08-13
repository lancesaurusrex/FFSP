using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

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

        public IEnumerable<StatsYearWeek> GetAllLivePlayers(int gid)
        {
            return _statsUpdate.GetAllLivePlayers(gid);
        }

        public IEnumerable<StatsYearWeek> GetAllHomePlayers(int gid) {
            return _statsUpdate.GetAllHomePlayers(gid);
        }

        public IEnumerable<StatsYearWeek> GetAllAwayPlayers(int gid) {
            return _statsUpdate.GetAllAwayPlayers(gid);
        }

        /********************************
         * ON AND OFF FOR "LIVE" ACTION
         * 
         *********************************/

        public string GetMarketState() {
            return _statsUpdate.StatsState.ToString();
        }

        public void OpenMarket() {
            _statsUpdate.OpenMarket();
        }

        public void CloseMarket() {
            _statsUpdate.CloseMarket();
        }

        public override System.Threading.Tasks.Task OnDisconnected(bool stopCalled) {
           
            if (stopCalled) {
                // We know that Stop() was called on the client,
                // and the connection shut down gracefully.
            }
            else {
                // This server hasn't heard from the client in the last ~35 seconds.
                // If SignalR is behind a load balancer with scaleout configured, 
                // the client may still be connected to another SignalR server.
            }

            return base.OnDisconnected(stopCalled);
        }
    }
}