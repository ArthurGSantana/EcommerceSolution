using System;
using EcommerceMinified.Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcommerceMinified.Data.Postgres.Configuration;

public class AddressConfiguration : IEntityTypeConfiguration<Address>
{
    public void Configure(EntityTypeBuilder<Address> builder)
    {
        builder.ToTable("Address");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.CustomerId)
            .HasColumnName("customer_id")
            .IsRequired();
        builder.Property(x => x.Street)
            .HasColumnName("address_street")
            .IsRequired()
            .HasMaxLength(100);
        builder.Property(x => x.Number)
            .HasColumnName("address_number")
            .IsRequired()
            .HasMaxLength(10);
        builder.Property(x => x.Complement)
            .HasColumnName("address_complement")
            .HasMaxLength(50);
        builder.Property(x => x.Neighborhood)
            .HasColumnName("address_neighborhood")
            .IsRequired()
            .HasMaxLength(50);
        builder.Property(x => x.City)
            .HasColumnName("address_city")
            .IsRequired()
            .HasMaxLength(50);
        builder.Property(x => x.State)
            .HasColumnName("address_state")
            .IsRequired()
            .HasMaxLength(2);
        builder.Property(x => x.ZipCode)
            .HasColumnName("address_zip_code")
            .IsRequired()
            .HasMaxLength(8);

        builder.HasOne<Customer>()
            .WithOne(x => x.Address)
            .HasForeignKey<Address>(x => x.CustomerId);
    }
}
