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
    public class GetInventoryService
    {
        public async void InventoryAsync(Socket socket, string command, string json, bool isEquipment)
        {
            try
            {
                DbConnect dbClient = new DbConnect();
                string jsonResult = command + '§' + await InventoryResult(json, isEquipment) + '±';
                // TODO добавить authToken
                Connector.Send(socket, jsonResult);
            }
            catch
            {
                Console.WriteLine("DB Error");
                Connector.Send(socket, command + '§' + "{\"result\":\"ERROR\",\"description\":\"error\"}±");
            }
        }
        public async Task<string> InventoryResult(string userData, bool isEquipment)
        {
            string characterId = DbConnect.JsonParseToken(userData, "_id");

            if (!string.IsNullOrEmpty(characterId))
            {
                var inventoryCollection = DbConnect.db_link.GetCollection<BsonDocument>("characterinventoryitemschemas");
                var builder = Builders<BsonDocument>.Filter;
                var filter = isEquipment ?
                    builder.Eq("characterId", characterId) & builder.Exists("itemEquipmentPlace")
                    :
                    builder.Eq("characterId", characterId);
                var dbPld = await inventoryCollection.Find(filter).ToListAsync();

                // если в бд есть запись
                if (dbPld != null)
                {
                    List<ItemModel> itemList = new List<ItemModel>();  
                      
                    foreach (var doc in dbPld)
                    {  
                        var parsedJson = DbConnect.JsonParser(doc, false);
                        itemList.Add(JsonConvert.DeserializeObject<ItemModel>(parsedJson));                       
                    }

                    var json = JsonConvert.SerializeObject(new ResponseModel<List<ItemModel>>(
                        result: "OK", description: null, data: itemList));

                    return json;
                }
                else
                {
                    return "{\"result\":\"ERROR\",\"description\":\"#InventoryNotFound\"}";
                }
            } 
            else 
            {
                return "{\"result\":\"ERROR\",\"description\":\"#InvalidData\"}";
            }
        }
    }
}