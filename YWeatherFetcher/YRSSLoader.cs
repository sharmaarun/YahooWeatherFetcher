using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;


namespace YWeatherFetcher
{

    
    /*
     * Use yahoo's weather rss feed api and load xml containing specific city's weather information
     */
    class YRSSLoader
    {
        
        String rssURL = "";
        ConfigLoader config = null;
        YCitiesLoader yCitiesLoader = null;
        URLScraper yScraper = null;

        XmlDocument doc = null;
        XmlNamespaceManager ns = null;
        XmlNode node = null;
        string currUrl = "";

        public YRSSLoader()
        {
            config = ConfigLoader.Instance;

            if (config != null)
            {
                if ((rssURL=config.Configuration["RSSURL"])==null)
                    Console.WriteLine("Error: Unable to fetch RSS URL from configuration!");
            }
            else
            {
                Console.WriteLine("Error: Unable to fetch configuration!");
            }

            yCitiesLoader = YCitiesLoader.Instance;

            if (URLScraper.Instance == null)
                yScraper = new URLScraper();
            else
                yScraper = URLScraper.Instance;

            doc = new XmlDocument();
            
        }


        /*
         * load states to cities map and load XML using URLScraper class
         * finally process xml and return araay of data
         */
        public ArrayList loadAndParseRSSXml(Dictionary<String,String> citiesMap)
        {
            ArrayList arrayOfData = new ArrayList();
            Console.WriteLine("\nScraping and parsing weather rss feed for all cities...");
            
            foreach(string state in citiesMap.Keys.ToArray())
            {
                Console.Write("\nFor " + state+" : \n");
                String[] currCitiesArray = citiesMap[state].Split(',');
                
                int percentDone = 0;
                string woeid = "";
               
                foreach (string city in currCitiesArray)
                {
                    
                        percentDone += 1;

                        Console.Write("\r" + ((int)((percentDone / (double)(currCitiesArray.Length)) * 100)) + "% done");

                        string[] tmp = city.Split('-');
                        foreach (string t in tmp)

                            woeid = tmp[tmp.Length - 1];

                        arrayOfData.Add(getMapFromRSSXml(state, woeid));
                    
                }

            }

            return arrayOfData;
        }

        /*
         *actual parsing of xml
         */
        public String[] getMapFromRSSXml(string state, string woeid)
        {
            String[] returnMap = new String[Int32.Parse(config.Configuration["TOTALPARAMS"])];
            
            currUrl = rssURL + "?w=" + woeid + "&u=" + config.Configuration["TEMPTYPE"];
            
            string xmlForCurrWoeid = yScraper.getDataFromURL(currUrl);

            string city, temp, lu, tt, title, pd, ctext, ctemp;
            string cityXPath = "/rss/channel/";
            string allXPath = "/rss/channel/item/";


            try
            {
                
                city = parseXMLGetValue(xmlForCurrWoeid, cityXPath, "title").Split('-')[1].Split(',')[0].Trim();
                temp = parseXMLGetValueWithAttr(xmlForCurrWoeid, allXPath, "yweather:condition", "temp");
                ctext = parseXMLGetValueWithAttr(xmlForCurrWoeid, allXPath, "yweather:condition", "text");
                ctemp = parseXMLGetValueWithAttr(xmlForCurrWoeid, allXPath, "yweather:condition", "temp");
                lu = DateTime.Now.ToString(@"M/d/yyyy hh:mm:ss tt");
                tt = config.Configuration["TEMPTYPE"];
                title = parseXMLGetValue(xmlForCurrWoeid, allXPath, "title");
                pd = parseXMLGetValue(xmlForCurrWoeid, allXPath, "pubDate");

                returnMap[0]= woeid;
                returnMap[1] = config.Configuration["COUNTRY"];
                returnMap[2] = state;
                returnMap[3] = city;
                returnMap[4] = temp;
                returnMap[5] = lu;
                returnMap[6] = tt;
                returnMap[7] = title;
                returnMap[8] = pd;
                returnMap[9] = ctext;
                returnMap[10] = ctemp;
            }
            catch (Exception e)
            {
                Console.WriteLine("Unable to fetch for : "+currUrl+" :: WOEID = " +woeid +"\n");
            }

            return returnMap;
        }


        /*
         * get value from xml using xpath and tagname
         */
        public String parseXMLGetValue(string xml, string xpath, string tagName)
        {
            if (!xml.Contains("Error"))
            {
                doc.LoadXml(xml);
                ns = new XmlNamespaceManager(doc.NameTable);
                ns.AddNamespace("yweather", "http://xml.weather.yahoo.com/ns/rss/1.0");
                node = doc.SelectSingleNode(xpath + "" + tagName, ns);

                return node.InnerText;
            }
            else
            {
                return null;
            }
        }

        /*
         * extension of parseXMLGetValue function to get attributes of tagnames
         */
        public String parseXMLGetValueWithAttr(string xml, string xpath, string tagName, string attr)
        {

            doc.LoadXml(xml);
            if (!xml.Contains("Error"))
            {
                ns = new XmlNamespaceManager(doc.NameTable);
                ns.AddNamespace("yweather", "http://xml.weather.yahoo.com/ns/rss/1.0");
                node = doc.SelectSingleNode(xpath + "" + tagName, ns);

                return node.Attributes[attr].InnerText;
            }
            else
            {
                return null;
            }
        }


        
    }
}
