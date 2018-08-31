
using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using Database;
using Models;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;

namespace Server
{
    public class WearCommandService
    { 
        public async void WearCommand(Socket socket, string command, string json)
        {
            try
            {
                DbConnect dbClient = new DbConnect();
                string jsonResult = command + '§' + await WearCommandResult(json, socket) + '±';
                // TODO добавить authToken
                Connector.Send(socket, jsonResult);
            }
            catch
            {
                Console.WriteLine("DB Error");
                Connector.Send(socket, command + '§' + "{\"result\":\"ERROR\",\"description\":\"error\"}±");
            }
        }

        private async Task<string> WearCommandResult(string userData, Socket socket)
        {
            var itemRequestResult = JsonConvert.DeserializeObject<ItemRequestResultModel>(userData);

            var collection = DbConnect.db_link.GetCollection<BsonDocument>("characters");

            var builder = Builders<BsonDocument>.Filter;

            var filter = builder.Eq("_id", ObjectId.Parse(itemRequestResult.characterId));

            var character = await collection.Find(filter).FirstOrDefaultAsync(); 

            collection = DbConnect.db_link.GetCollection<BsonDocument>("characterinventoryitemschemas");
            
            filter = builder.Eq("_id", ObjectId.Parse(itemRequestResult.itemId)) & builder.Eq("characterId", itemRequestResult.characterId);
            // шмотка действительно есть ли у персонажа
            var itemFromPlayer = await collection.Find(filter).FirstOrDefaultAsync();

            var parsedJsonItemFromPlayer = DbConnect.JsonParser(itemFromPlayer, false);
            var parsedJsonCharacter = DbConnect.JsonParser(character, true);

            var item = JsonConvert.DeserializeObject<ItemModel>(parsedJsonItemFromPlayer);
            var baseCharacter = JsonConvert.DeserializeObject<BaseCharacterModel>(parsedJsonCharacter);

            // если в бд есть запись
            if (itemFromPlayer != null)
            {
                if (item.baseItem.itemLvl <= baseCharacter.characterLvl)
                {
                    if (item.baseItem.characterClassId == baseCharacter.characterClassId) 
                    {   
                        // TODO переделать под запрос с 45 строкой
                        filter = builder.Eq("itemEquipmentPlace", item.baseItem.itemType) & builder.Eq("characterId", baseCharacter._id);
                        var itemEquiped = await collection.Find(filter).FirstOrDefaultAsync();
                        
                        if (itemEquiped != null)
                        {
                            var updateEquipedItem = Builders<BsonDocument>.Update.Unset("itemEquipmentPlace");
                            var resultUpdateEquipedItem = await collection.UpdateOneAsync(filter, updateEquipedItem);
                        }
                
                        var updateItemFromPlayer = Builders<BsonDocument>.Update.Set("itemEquipmentPlace", item.baseItem.itemType);
                        var resultUpdateItemFromPlayer = await collection.UpdateOneAsync(filter, updateItemFromPlayer);
                        
                        GetInventoryService getInventory = new GetInventoryService();
                        //должны отдать новый слепок героя
                        return await getInventory.InventoryResult("{\"_id\" : \"" + baseCharacter._id + "\"}", false);
                    }
                    else 
                    {
                        return "{\"result\":\"ERROR\",\"description\":\"#ItemWearErrorClass\"}";
                    }                
                } 
                else 
                {
                    return "{\"result\":\"ERROR\",\"description\":\"#ItemWearErrorLevel\"}";
                }
            } 
            else 
            {
                return "{\"result\":\"ERROR\",\"description\":\"#InvalidData\"}";
            }
        }

        public async void UnwearCommand(Socket socket, string command, string json)
        {
            try
            {
                DbConnect dbClient = new DbConnect();
                string jsonResult = command + '§' + await UnwearCommandResult(json, socket) + '±';
                // TODO добавить authToken
                Connector.Send(socket, jsonResult);
            }
            catch
            {
                Console.WriteLine("DB Error");
                Connector.Send(socket, command + '§' + "{\"result\":\"ERROR\",\"description\":\"error\"}±");
            }
        }

        private async Task<string> UnwearCommandResult(string userData, Socket socket)
        {
            var itemRequestResult = JsonConvert.DeserializeObject<ItemRequestResultModel>(userData);

            var collection = DbConnect.db_link.GetCollection<BsonDocument>("characters");
            var builder = Builders<BsonDocument>.Filter;
            var filter = builder.Eq("_id", ObjectId.Parse(itemRequestResult.characterId));
            var character = await collection.Find(filter).FirstOrDefaultAsync();

            collection = DbConnect.db_link.GetCollection<BsonDocument>("characterinventoryitemschemas");
            filter = builder.Eq("_id", ObjectId.Parse(itemRequestResult.itemId)) & builder.Eq("characterId", itemRequestResult.characterId);
            // шмотка действительно есть ли у персонажа
            var itemFromPlayer = await collection.Find(filter).FirstOrDefaultAsync();

            var item = JsonConvert.DeserializeObject<ItemModel>(parsedJsonItemFromPlayer);
            
            // если в бд есть запись
            if (itemFromPlayer != null)
            {
                var updateItemFromPlayer = Builders<BsonDocument>.Update.Unset("itemEquipmentPlace");
                var resultUpdateItemFromPlayer = await collection.UpdateOneAsync(filter, updateItemFromPlayer);
            }
            else
            {
                return "{\"result\":\"ERROR\",\"description\":\"#InvalidData\"}";
            }
        }
    }
}