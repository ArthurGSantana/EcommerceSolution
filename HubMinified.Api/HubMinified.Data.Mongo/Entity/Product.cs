using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HubMinified.Data.Mongo.Entity;

public class Product : EntityBase
{
    [BsonRepresentation(BsonType.String)]
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal ProductWeight { get; set; }
}
