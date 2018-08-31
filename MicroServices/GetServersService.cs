using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;
using Database;
using Models;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;

namespace Server
{
    public class GetServersService
    {           
        public async void GetServers(Socket socket, string command)
        {
            try
            {
                DbConnect dbClient = new DbConnect();
                string json = command + '§' + await GetServers() + '±';
                Connector.Send(socket, json);
            }
            catch
            {
                Console.WriteLine("Db Error");
                Connector.Send(socket, command + '§' + "Error±");
            }
           
        }
        private async Task<string> GetServers()
        {
            var collection = DbConnect.db_link.GetCollection<BsonDocument>("servers");
            var filter = new BsonDocument();
            var servers = await collection.Find(filter).ToListAsync();
            List<ServerModel> serverModelList = new List<ServerModel>();  
                      
            foreach (var doc in servers)
            {            
                var parsedJson = DbConnect.JsonParser(doc, false);
                serverModelList.Add(JsonConvert.DeserializeObject<ServerModel>(parsedJson));
            }  

            var json = JsonConvert.SerializeObject(new ResponseModel<List<ServerModel>>(
                result: "OK", description: null, data: serverModelList));
            
            return json; 
        } 
    }
}