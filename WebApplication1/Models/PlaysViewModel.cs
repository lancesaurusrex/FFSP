using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication1.Models {


        public class PlaysVM {//Start\Key is a number that I have not figured out what it means.
            public int PlayNum { get; set; }

            [JsonProperty("sp")]
            public int Sp { get; set; }

            [JsonProperty("qtr")]
            public int Qtr { get; set; }

            [JsonProperty("down")]
            public int Down { get; set; }

            [JsonProperty("time")]
            public string Time { get; set; }

            [JsonProperty("yrdln")]
            public string Yrdln { get; set; }

            [JsonProperty("ydstogo")]
            public int Ydstogo { get; set; }

            [JsonProperty("ydsnet")]
            public int Ydsnet { get; set; }

            [JsonProperty("posteam")]
            public string Posteam { get; set; }

            [JsonProperty("desc")]
            public string Desc { get; set; }

            [JsonProperty("note")]
            public string Note { get; set; }

            public string EPState { get; set; }
            public double MarkovExpPts { get; set; }

            [JsonIgnoreAttribute]
            public IList<PlayersVM> Players { get; set; }    
        }

        public class PlayersVM {//The "Start"/Key of this JSONArray is the playerID, each one is different
            public int PlayNum { get; set; }

            //Add PlayerID! from data extraction.

            [JsonProperty("sequence")]
            public int Sequence { get; set; }

            [JsonProperty("clubcode")]
            public string Clubcode { get; set; }

            [JsonProperty("playerName")]
            public string PlayerName { get; set; }

            [JsonProperty("statId")]
            public int StatId { get; set; }

            [JsonProperty("yards")]
            public int Yards { get; set; }
        }
    }
