using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;


namespace YWeatherFetcher
{
    // Startup Configuration laoder
    // uses singleton structure to be used as single instance throughout the app
    public class ConfigLoader
    {
        //Singleton architecture

        private static ConfigLoader instance;

        // Configuration variables
        private String configPath = "";
        private Dictionary<String, String> configMap = null;

        public ConfigLoader(String pathToConfig)
        {
            this.configPath = pathToConfig;
            ConfigLoader.instance = this;
        }

        /*
         * load configuration file
         */
        public bool LoadConfig()
        {
            if (this.configPath != "" && this.configPath!=null)
            {
               
                
                try
                {
                    String[] configStr = File.ReadAllLines(@""+configPath);
                    this.configMap = new Dictionary<string, string>();
                    for (int i = 0; i < configStr.Length; i++)
                    {
                        
                        
                        if (!(configStr[i].Trim().Substring(0, 1).Equals("#")))
                        {
                            
                            this.configMap.Add(configStr[i].Split('=')[0], configStr[i].Split('=')[1]);
                        }

                        
                        
                    }

                    

                }
                catch (FileNotFoundException e)
                {
                    Console.Write("Configuration Error : File '"+this.configPath+"' Could not be found!\n"+e);
                    Console.Read();
                }
                catch (Exception e)
                {
                    Console.Write(e);
                    Console.Read();
                }



                
                return true;
            }
            else
            return false;
        }




        // getter setters

        public static ConfigLoader Instance
        {
            get
            {
                return instance;
            }
        }

        public Dictionary<String, String> Configuration
        {
            get
            {
                return this.configMap;
            }
        }

        

    }
}
