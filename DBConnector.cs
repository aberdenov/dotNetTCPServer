using System;
using System.Threading.Tasks;
using Models;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using System.IO;
using System.Collections.Generic;
using MongoDB.Bson.Serialization;
using Newtonsoft.Json.Linq;
using System.Net.Sockets;

namespace Database 
{
    class DbConnect 
    {
        //статика или созадавать соединение при каждом запросе?
        public static IMongoDatabase db_link;
        public static void Connect ()
         {
            try 
            {
                string connectionString = "mongodb://localhost:27017"; // данные для доступа
                string dbName = "gg"; // имя базы данных                
                
                MongoClient client = new MongoClient (connectionString);
                db_link = client.GetDatabase (dbName);
            } 
            catch (MongoConnectionException e) 
            {
                Console.WriteLine (e);
            }

        }
        public static string JsonParser(BsonDocument json, bool isDate)
        {
            string parsedJson = isDate ? 
                json.ToString().Replace("ObjectId(", string.Empty).Replace(")", String.Empty).Replace("ISODate(", string.Empty)
                :
                json.ToString().Replace("ObjectId(", string.Empty).Replace(")", String.Empty);

            return parsedJson;
        } 

        public static string JsonParseToken(string json, string field)
        {
            JObject jo = JObject.Parse(json);
            JToken jToken = jo[field];
            return (string)jToken;
        }     
    }
}