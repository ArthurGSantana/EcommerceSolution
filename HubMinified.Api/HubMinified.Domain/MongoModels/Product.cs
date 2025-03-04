using System;

namespace HubMinified.Domain.MongoModels;

public class Product : ModelBase
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal ProductWeight { get; set; }
}
