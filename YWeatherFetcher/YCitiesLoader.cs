using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;



namespace YWeatherFetcher
{

    /**
     * Loads Country->States->Cities list
     * from yahoo weather url using scraping
     */
    class YCitiesLoader
    {

        private static YCitiesLoader instance;

        String mainURL = "";
        int startCountForStates = 4; // to ignore extra content while scraping
        ConfigLoader config = null; 
        URLScraper yScraper = null;
        Dictionary<String, String> statesToCitiesMap = null;

        public YCitiesLoader()
        {
            instance = this;
            config = ConfigLoader.Instance;

            if (config != null)
                config.Configuration.TryGetValue("COUNTRYURL", out mainURL);
            
            if(mainURL=="" || mainURL==null )
            mainURL = "http://in.weather.yahoo.com/india/";

            
            statesToCitiesMap = new Dictionary<string, string>();

            if (URLScraper.Instance == null)
                yScraper = new URLScraper();
            else
                yScraper = URLScraper.Instance;
        }

        // loads cities using scraping method
        public bool loadCities()
        {
            
           /*
           * Here we performe scraping operation and then parse the HTML data using regex
           * Note: alternative for parsing can be usage of HTML Agility Pack[open source].
           */
            try
            {
                Console.WriteLine("Loading all the States and Cities...");
                string statesString = yScraper.getDataFromURL(mainURL);


                //use regex to get the data between HTML tags that hold all the states
                string strRegex = @"<div class=""yom-mod(.+?)div id=""reg-pg"">";
                Regex myRegex = new Regex(strRegex, RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.ECMAScript | RegexOptions.CultureInvariant);
                String excerpt = (myRegex.Match(statesString).Groups[1].Value);

                //again use regex to extract only the states [~along with 2 or 3 extra sentences]
                strRegex = @"(?<=^|>)[^><]+?(?=<|$)";
                Regex myRegexStates = new Regex(strRegex, RegexOptions.IgnoreCase | RegexOptions.Singleline);
                MatchCollection matches = myRegexStates.Matches(@excerpt);

                // now we need to perform scraping once more to gather all the cities and their woeids
                // procedure is kind of similar
                

                int tempCount = 0;
                double percentDone = 0;// for accessing the match collection from right position.
               
                foreach (Match m in matches)
                {
                    
                        tempCount++;

                        if (tempCount > startCountForStates)
                        {
                            percentDone += 1;
                            Console.Write("\r" + ((int)((percentDone / (double)(matches.Count - startCountForStates)) * 100)) + "% done");

                            string citiesString = yScraper.getDataFromURL(mainURL + "" + m.ToString());


                            String strRegexCities = @"<div class=""yom-mod yom-weather-region""(.+?)<div id=""reg-pg"">";
                            Regex myRegexCities = new Regex(strRegexCities, RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.ECMAScript | RegexOptions.CultureInvariant);
                            String excerptCities = (myRegexCities.Match(citiesString).Groups[1].Value);

                            strRegexCities = @"<a href=""/" + config.Configuration["COUNTRY"].ToLower() + "/" + m.ToString().Replace(" ", "-") + @"/(.+?)/"">";

                            Regex myRegexStatesCities = new Regex(strRegexCities, RegexOptions.IgnoreCase | RegexOptions.Singleline);
                            MatchCollection matchesCities = myRegexStatesCities.Matches(excerptCities);

                            String tempStr = "";

                            foreach (Match mm in matchesCities)
                            {
                                
                                int i = mm.ToString().LastIndexOf("/");

                                int j = mm.ToString().Substring(0, i - 1).LastIndexOf("/");

                                string str = mm.ToString().Substring(j + 1, (i - j)-1);

                                tempStr += str + ",";
                                
                            }
                            tempStr = tempStr.Substring(0, tempStr.Length - 1);

                            // add cities and states to dictionary for future use
                            statesToCitiesMap.Add(m.ToString(), tempStr);



                        }
                    

                }
                return true;

            }
            catch (Exception e)
            {
                Console.WriteLine("Error occured : \n" + e);
            }
            

            return false;
        }


        public Dictionary<String, String> StatesCitiesMap
        {
            get
            {
                return statesToCitiesMap;
            }
        }

        public static YCitiesLoader Instance
        {
            get
            {
                return instance;
            }
        }



       
    }
}
