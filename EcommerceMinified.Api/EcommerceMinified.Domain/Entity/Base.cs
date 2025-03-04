using System;

namespace EcommerceMinified.Domain.Entity;

public class Base
{
    public Guid Id { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }

    public Base()
    {
        Id = Guid.NewGuid();
        CreatedDate = DateTime.UtcNow;
    }

    public virtual void Create()
    {
        CreatedDate = DateTime.UtcNow;
    }

    public virtual void Update()
    {
        UpdatedDate = DateTime.UtcNow;
    }
}
