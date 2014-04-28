using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace YWeatherFetcher
{

    /*
     * Database access class
     */
    class YDBAccess
    {

        String connectionString = "";
        String table = "table_weather_info";
        OleDbConnection connection = null;
        OleDbCommand cmd = null;
        String query = "";
        DataSet ds = null;
        OleDbDataAdapter da = null;

        public YDBAccess()
        {
            connection = new OleDbConnection();
        }

        public YDBAccess(String dbPath)
        {
            connectionString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data"
                + @" Source="+dbPath;
            connection = new OleDbConnection();
            connection.ConnectionString = connectionString;
            cmd = new OleDbCommand();
            cmd.CommandType = CommandType.Text;
            cmd.Connection = connection;
        }

        /*
         * Truncates and the updates database table for refreshing purpose
         */
        public bool updateDBWithMapArray(ArrayList dbMap)
        {
            try
            {
                connection.Open();
                cmd.CommandText = "DELETE FROM " + table+" ;";
                cmd.ExecuteNonQuery();
                Console.WriteLine("Cleared previous records...");
               
                foreach(String[] strarr  in dbMap)
                {
                    if (strarr[0] == null || strarr[0].Equals(""))
                        continue;

                    cmd.CommandText = @" INSERT INTO " + table + " (ID,COUNTRY,STATE,CITY,TEMPERATURE,LAST_UPDATED,T_TYPE,TITLE,PUBLISH_DATE,CONDITION_TEXT,CONDITION_TEMP) ";
                    
                    string substr = "";
                    cmd.CommandText += @" values(";
                    for (int i = 0; i < strarr.Length; i++)
                    {
                        substr += "'" + strarr[i] + "',";
                    }
                    cmd.CommandText += substr.Substring(0, substr.Length - 1) + ") ";
                    cmd.CommandText += " ;";
                    
                    cmd.ExecuteNonQuery(); 
                }

                Console.WriteLine("Updated database records successfully...");
                connection.Close();

                
            }
            catch (Exception e)
            {
                Console.WriteLine("Unable to connect/execute query!\n" + e);
                return false;
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                    connection.Close();
            }



            return true;
        }



        // return all states currently in DB
        public string[]  getAllStatesFromDB()
        {
            string[] arr = null;

            string queryn = @"Select distinct State from "+table+" ;";
            DataRowCollection rows = getRowsByQuery(queryn, "Unable to get state!");
            arr = new string[rows.Count];
                for (int i = 0; i < arr.Length; i++)
                {
                    arr[i] = (string)rows[i]["state"];
                }

            return arr;
        }




        // return all cities for specified state currently in DB
        public string[] getAllCitiesFromDBForState(string state)
        {
            string[] arr = null;
            string queryn = @"Select city from " + table + " where STATE = '" + state + "' ;";
            DataRowCollection rows = getRowsByQuery(queryn, "Unable to get cities for state: " + state);
            arr = new string[rows.Count];
                for (int i = 0; i < arr.Length; i++)
                {
                    arr[i] = (string)rows[i]["city"];
                }
            return arr;
        }

        // return woeid for city from DB
        public string getWOEIDByCity(string city)
        {
            string queryn = @"Select id from " + table + " where city = '" + city + "' ;";
            DataRowCollection rows = getRowsByQuery(queryn, "Unable to get woeid for city: " + city);
            return (string)rows[0]["id"];
            
        }

        // show all data for a particular city
        public string showAllDataForCity(string code)
        {
            Regex regex = new Regex(@"^\d+$");
            string woeid = "";
            if (regex.IsMatch(code))
            {
                woeid = code;
            }
            else
            {
                woeid = getWOEIDByCity(code);
            }
            string status = "";
            string queryn = @"Select * from " + table + " where id = '" + woeid + "' ;";
            DataRowCollection rows = getRowsByQuery(queryn, "Unable to get data for : " + code);


            status += "[Temperature : " + (string)rows[0]["TEMPERATURE"] + "" + (string)rows[0]["T_TYPE"] + ";  ";
            status += "Last Updated on : " + (string)rows[0]["LAST_UPDATED"] + ";  ";
            status += "Description : " + (string)rows[0]["TITLE"] + ";  ";
            status += "Result published on : " + (string)rows[0]["PUBLISH_DATE"] + ";  ";
            status += "Current Condition : " + (string)rows[0]["CONDITION_TEXT"] + "]  ";


            return status;
        }

        

        // returns rows of data based on specified query
        private DataRowCollection getRowsByQuery(string query,string errorStr)
        {
            try
            {
                if (connection.State == ConnectionState.Closed)
                    connection.Open();
                this.query = query;
                ds = new DataSet();
                da = new OleDbDataAdapter(query, connection);
                da.Fill(ds, table);


                return ds.Tables[table].Rows;
            }
            catch (Exception e)
            {
                Console.WriteLine(errorStr);
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                    connection.Close();
            }

            return null;
        }


        public string ConnectionString
        {
            get
            {
                return connectionString;
            }

            set
            {
                this.connectionString = value;
            }
        }

        


        

    }
}
