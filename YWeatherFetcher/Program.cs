using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

/**
 * Program entrance.
 */

namespace YWeatherFetcher
{
    class Program
    {
        static void Main(string[] args)
        {
            //Load Configuration File
            ConfigLoader m_Config = new ConfigLoader("config.txt");
            m_Config.LoadConfig();

            
                
            
            //create required instances
            YCitiesLoader m_citiesLoader = new YCitiesLoader();
            YRSSLoader m_rssLoader = new YRSSLoader();
            YDBAccess m_dbAccess = new YDBAccess(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)+"/YWeatherDB.mdb");
            

            int option = 0;
            String rl = "";
            while (option != 4)
            {
                // Menu 
                Console.WriteLine("\n================================================================");
                Console.WriteLine("---------------  Yahoo Weather Scraper  ------------------------");
                Console.WriteLine("================================================================");
                Console.WriteLine("\nMenu:");
                Console.WriteLine("1) Refresh Databse - Load states,cities,temperature etc into DB.");
                Console.WriteLine("2) Show Temperature for all Cities [From DB]");
                Console.WriteLine("3) Show Temperature for specific City [From DB]");
                Console.WriteLine("4) Exit");
                Console.Write("Please choose an option : ");
                rl = Console.ReadLine();
                if (rl == null || rl.Equals(""))
                    return;
                option = Int32.Parse(rl);
                switch (option)
                {
                    case 1:
                        //load states-cities
                        m_citiesLoader.loadCities();                        
                        //load rss xml for each city
                        ArrayList tmp = m_rssLoader.loadAndParseRSSXml(m_citiesLoader.StatesCitiesMap);
                        //store fetched values {from rss xml} to database
                        m_dbAccess.updateDBWithMapArray(tmp);
                        continue;
                    case 2:
                        // load all the states->cities->weather info
                        foreach (string str in m_dbAccess.getAllStatesFromDB())
                        {
                            Console.WriteLine();
                            Console.WriteLine("\n"+str+":");
                            Console.WriteLine("===========================================");
                            foreach (string strr in m_dbAccess.getAllCitiesFromDBForState(str))
                            {
                                
                                Console.WriteLine(strr+":");
                                Console.WriteLine(m_dbAccess.showAllDataForCity(strr));
                            }
                        }
                        Console.WriteLine("\n");
                        continue;
                    case 3:
                        // show weather info for specific city
                        string input = "";
                        Console.WriteLine("Do you want to get all city names enlisted?(y/n)");
                        input = Console.ReadLine();
                        if(input.ToLower().Equals("y"))
                        {
                            foreach (string str in m_dbAccess.getAllStatesFromDB())
                            {
                                Console.WriteLine();
                                Console.WriteLine("\n" + str + "");
                                Console.WriteLine("===========================================");
                                foreach (string strr in m_dbAccess.getAllCitiesFromDBForState(str))
                                {

                                    Console.WriteLine(strr + "");
                                    
                                }
                            }
                            Console.WriteLine("\n");

                        }
                        else
                        {
                            Console.WriteLine("Please enter WOEID or City's exact name:");
                            input = Console.ReadLine();
                            Console.WriteLine(m_dbAccess.showAllDataForCity(input));
                            
                        }
                        continue;
                    case 4:
                        //exit
                        break;
                    
                        
                }
            }
            /*
          
             * */
            
        }
    }
}
