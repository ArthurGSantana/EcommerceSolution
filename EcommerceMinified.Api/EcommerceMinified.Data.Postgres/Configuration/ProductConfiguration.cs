using System;
using EcommerceMinified.Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcommerceMinified.Data.Postgres.Configuration;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Product");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasColumnName("product_name")
            .IsRequired()
            .HasMaxLength(100);
        builder.Property(x => x.Description)
            .HasColumnName("product_description")
            .IsRequired()
            .HasMaxLength(500);
        builder.Property(x => x.Price)
            .HasColumnName("product_price")
            .IsRequired();
        builder.Property(x => x.Stock)
            .HasColumnName("product_stock")
            .IsRequired();
        builder.Property(x => x.Category)
            .HasColumnName("product_category")
            .HasConversion<int>()
            .IsRequired();
        builder.Property(x => x.Image)
            .HasColumnName("product_image");
    }
}
