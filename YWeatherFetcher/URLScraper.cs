using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace YWeatherFetcher
{
    // main scraping class
    class URLScraper
    {
        private static URLScraper instance;
        string urlToLoad = "";
        HttpWebRequest webRequest;
        HttpWebResponse webResponse;
        Stream webResponseStream;

        public URLScraper()
        {
            instance = this;
        }


        /*
         * returns a string of web content by loading given url
         */
        public string getDataFromURL(string mainURL)
        {
            if (mainURL == null)
            {
                return null;
            }



            urlToLoad = mainURL;
            StringBuilder sb = new StringBuilder();
            byte[] buf = new byte[8192];
            try
            {
                // create request to get web content
                webRequest = (HttpWebRequest)WebRequest.Create(urlToLoad);
                webResponse = (HttpWebResponse)webRequest.GetResponse();
                // use the response and convert it to string
                webResponseStream = webResponse.GetResponseStream();

                String tmp = "";
                int count = 0;
                do
                {
                    count = webResponseStream.Read(buf, 0, buf.Length);

                    if (count != 0)
                    {
                        tmp = Encoding.ASCII.GetString(buf, 0, count);
                        sb.Append(tmp);
                    }
                }
                while (count > 0);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error:\n" + e);
            }
            //return final string
            return sb.ToString();
        }



        public static URLScraper Instance
        {
            get
            {
                return instance;
            }
        }


        public string CurrentURL
        {
            get
            {
                return urlToLoad;
            }
        }


    }
}
