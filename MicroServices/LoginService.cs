using System;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using Database;
using Models;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Server
{
    public class LoginService
    {           
        public async void LoginAsync(Socket socket, string command, string json)
        {
            try
            {
                DbConnect dbClient = new DbConnect();
                string jsonResult = command + '§' + await LoginResult(json) + '±';
                // TODO добавить authToken
                Connector.Send(socket, jsonResult);
            }
            catch
            {
                Console.WriteLine("DB Error");
                Connector.Send(socket, command + '§' + "{\"result\":\"ERROR\",\"description\":\"error\"}±");
            }
           
        }
        private async Task<string> LoginResult(string userData)
        {
            var authModel = JsonConvert.DeserializeObject<AuthModel>(userData);

            if(!string.IsNullOrEmpty(authModel.email) && !string.IsNullOrEmpty(authModel.password))
            {
                // еще надо добавить проверок
                //получаем таблицу из БД
                var userCollection = DbConnect.db_link.GetCollection<BsonDocument>("users");
                var builder = Builders<BsonDocument>.Filter;
                var filter = builder.Eq("email", authModel.email) & builder.Eq("password", authModel.password);
                // приминяем фильтр и получаем результат
                var dbPld = await userCollection.Find(filter).Project("{_id: 1}").FirstOrDefaultAsync();
                // если в бд есть запись
                if(dbPld != null)
                {
                    var parsedDbPld = DbConnect.JsonParser(dbPld, false); 
                    return await Login(parsedDbPld, authModel);
                }
                else
                {
                    //временная заглушка
                    return "{\"result\":\"ERROR\",\"description\":\"#UserNotFound\"}";
                    // return await Registration(authModel, userCollection);
                }
            }
            else
            {
                return "{\"result\":\"ERROR\",\"description\":\"#InvalidData\"}";
            }
        }
        private async Task<string> Registration(AuthModel userData, IMongoCollection<BsonDocument> collection)
        {
            if (!string.IsNullOrEmpty(userData.email) && 
            !string.IsNullOrEmpty(userData.password))
            {
                // проверяем есть ли пользователь с таким email адресом в базе
                BsonDocument player = new BsonDocument
                {
                    {"email", userData.email},
                    {"password", userData.password},
                    {"createdAt", DateTime.UtcNow}
                };

                await collection.InsertOneAsync(player);

                var parsedDbPld = DbConnect.JsonParser(player, true);

                return await Login(parsedDbPld, userData);
            }
            else
            {
                return "{\"result\":\"ERROR\",\"description\":\"#InvalidData\"}";
            } 

        }
        private async Task<string> Login(string parsedJson, AuthModel userData)
        {
            string _id = DbConnect.JsonParseToken(parsedJson, "_id");

            var charCollection = DbConnect.db_link.GetCollection<AuthModel>("characters");
            var builder = Builders<AuthModel>.Filter;
            var filter = builder.Eq("userId", _id) & builder.Eq("serverId", userData.serverId);
            //поиск characterId
            var characterId = await charCollection.Find(filter).Project("{_id: 1}").FirstOrDefaultAsync();

            LoginResultModel loginResult = new LoginResultModel();
            if(characterId != null)
            {
                var parsedDbPld = DbConnect.JsonParser(characterId, false);
                loginResult = JsonConvert.DeserializeObject<LoginResultModel>(parsedDbPld);
            }

            Guid guid = Guid.NewGuid();
            string auth_key = guid.ToString();
            loginResult.auth_key = auth_key;
            var json = JsonConvert.SerializeObject(new ResponseModel<LoginResultModel>(result: "OK", description: null, data: loginResult));
            return json;
        }
    }
}