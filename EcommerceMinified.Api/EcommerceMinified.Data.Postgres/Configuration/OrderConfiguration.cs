using System;
using EcommerceMinified.Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcommerceMinified.Data.Postgres.Configuration;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Order");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.CustomerId)
            .HasColumnName("customer_id")
            .IsRequired();
        builder.Property(x => x.Total)
            .HasColumnName("order_total")
            .IsRequired();
        builder.Property(x => x.Status)
            .HasColumnName("order_status")
            .HasConversion<int>()
            .IsRequired();
        builder.Property(x => x.OrderDate)
            .HasColumnName("order_date");

        builder.HasOne<Customer>()
            .WithMany()
            .HasForeignKey(x => x.CustomerId);

        builder.HasMany(x => x.Items)
            .WithOne()
            .HasForeignKey(x => x.OrderId);
    }
}