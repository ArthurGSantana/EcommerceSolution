using System;
using EcommerceMinified.Domain.Interfaces.Commands;

namespace EcommerceMinified.MsgContracts.Command;

public class ProductInfoCommand : IProductInfoCommand
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal ProductWeight { get; set; }
}
