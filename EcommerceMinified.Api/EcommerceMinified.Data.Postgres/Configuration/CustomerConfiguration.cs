using System;
using EcommerceMinified.Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcommerceMinified.Data.Postgres.Configuration;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customer");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasColumnName("customer_name")
            .IsRequired()
            .HasMaxLength(100);
        builder.Property(x => x.Email)
            .HasColumnName("customer_email")
            .IsRequired()
            .HasMaxLength(100);
        builder.Property(x => x.Password)
            .HasColumnName("customer_password")
            .IsRequired()
            .HasMaxLength(100);
        builder.Property(x => x.Phone)
            .HasColumnName("customer_phone")
            .HasMaxLength(20);
        builder.Property(x => x.Image)
            .HasColumnName("customer_image");

        builder.HasOne(x => x.Address)
            .WithOne()
            .HasForeignKey<Address>(x => x.CustomerId);
    }
}
