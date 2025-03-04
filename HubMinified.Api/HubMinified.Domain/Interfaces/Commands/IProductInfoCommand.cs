using System;

namespace EcommerceMinified.Domain.Interfaces.Commands;

public interface IProductInfoCommand
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; }
    public decimal ProductWeight { get; set; }
}
