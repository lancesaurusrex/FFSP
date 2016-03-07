using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace WebApplication1.Models {
    public class NFLLiveViewModel {

        public void dostuff() {

            String FileName = "2015101200_gtd.json";
            ReadJSONDatafromNFL r = new ReadJSONDatafromNFL(FileName);
            JObject indPlay = null;
            while (r.endOfGame == false) {
                r.QuickDe(FileName);
                if (r.isUpdate == true)
                indPlay = r.play;
            }
        }

        public void sendPlay(JObject play) {

            //dosomethingwith play
        }
    }
}