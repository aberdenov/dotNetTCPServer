using System;
using MongoDB.Bson.Serialization.Attributes;

namespace Models
{
    public class AuthModel
    {
        public string _id { get; set; }
        public string email { get; set; }
        public string password { get; set; }
        public string serverId { get; set; }
        public DateTime createdAt {get;set;}
    }
}
