using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApplication1.DAL;

/* Purpose: To take schedule dates and turn them into a NFL.com gameID by currWeek and currYear
 * NFLGameID - ex:YYYYMMDD##, game count is the last 2 numbers, starts at 00 and is incremented for each game on that day
 * 
 * Return: A List of strings called NFLGamesID, will go through every day of the selected week, year and put into a list for that week, year.
 */

namespace WebApplication1.Models {
    public class NFLWeekSetup {
        public NFLWeekSetup(int currWeek, int currYear) { currentWeek = currWeek; currentYear = currYear; }
        public List<NFLGame> NFLGamesByWeek;
        public List<string> NFLGamesID;
        public int currentWeek;    //current passed in through constructor
        public int currentYear;    //current passed in through constructor 

        //pulls week schedule into List NFLGamesByWeek
        public void PullWeekSchedule() {
            using (var db = new FF()) {
                NFLGamesByWeek = db.NFLGame.Where(g => Convert.ToInt16(g.Week) == currentWeek && g.Year == currentYear).ToList();
            }
        }
        //Sort LIst by Day Number? how to do that
        public void SortGamesByDay() {
            NFLGamesByWeek.Sort((x, y) => x.DateEST.Day.CompareTo(y.DateEST.Day));  // Dec27(FirstList) > Dec30 > Dec31 > Jan3(LastList)
            //Do num of games by day count in sort?
        }

        public List<string> CreateNFLcomIDs() {
            var firstDay = NFLGamesByWeek.First().DateEST;
            var lastDay = NFLGamesByWeek.Last().DateEST;
            List<NFLGame> results;

            foreach (var d in EachDay(firstDay, lastDay)) {
                results = NFLGamesByWeek.FindAll(
                delegate(NFLGame ng)
                {
                    return ng.DateEST == d;
                }
               );

                int NFLnum = 0;     //For end of NFL.com game id ex:YYYYMMDD##

                if (results != null) {
                    foreach (var g in results) {
                        var numString = NFLnum.ToString().PadLeft(NFLnum.ToString().Length, '0');

                        NFLGamesID.Add(g.DateEST.ToString() + numString);
                        ++NFLnum;   //add pattern is first run is 00, 01, 02, 03...
                    }
                }
            }
            return NFLGamesID;
        }

        //Loop through DateTime Days help method
        public IEnumerable<DateTime> EachDay(DateTime from, DateTime thru) {
            for (var day = from.Date; day.Date <= thru.Date; day = day.AddDays(1))
                yield return day;
        }

        //Converts DateTime Date into yyyyMMdd string format
        public string convertDateTimeToyyyyMMdd(DateTime NFLGameTime) {

            string dt = NFLGameTime.ToString("yyyyMMdd");
            return dt;
        }

        public void ConvertDTToNFLID(NFLGame n) {
            //Count numbers of games by day
            convertDateTimeToyyyyMMdd(n.DateEST);
        }
    }
}