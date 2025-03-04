using System;
using EcommerceMinified.Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcommerceMinified.Data.Postgres.Configuration;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("OrderItem");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.OrderId)
            .HasColumnName("order_id")
            .IsRequired();
        builder.Property(x => x.ProductId)
            .HasColumnName("product_id")
            .IsRequired();
        builder.Property(x => x.Quantity)
            .HasColumnName("item_quantity")
            .IsRequired();
        builder.Property(x => x.Price)
            .HasColumnName("item_price")
            .IsRequired();

        builder.HasOne<Order>()
            .WithMany(x => x.Items)
            .HasForeignKey(x => x.OrderId);
        builder.HasOne<Product>()
            .WithMany()
            .HasForeignKey(x => x.ProductId);
    }
}
