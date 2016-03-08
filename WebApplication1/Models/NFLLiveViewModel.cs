using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace WebApplication1.Models {
    public class NFLLiveViewModel {
        public JObject _play { get; set; }
        public FFTeam HomeTeam { get; set; }
        public FFTeam AwayTeam { get; set; }

        public void dostuff() {

            String FileName = "2015101200_gtd.json";
            ReadJSONDatafromNFL r = new ReadJSONDatafromNFL(FileName);
            JObject indPlay = null;
            while (r.endOfGame == false) {
                r.QuickDe(FileName);
                if (r.isUpdate == true)
                indPlay = r.play;
                SetPlay(r.play);
            }
        }

        public void SetPlay(JObject play) {
            _play = play;
            
        }

        public JObject GetPlay()
        {
            return _play;
        }
    }
}