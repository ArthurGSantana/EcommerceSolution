using System;
using System.Text.Json.Serialization;

namespace HubMinified.Domain.MongoModels;

public class ModelBase
{
    public string Id { get; set; } = string.Empty;
    [JsonIgnore]
    public DateTime CreatedDate { get; set; }
    [JsonIgnore]
    public DateTime? UpdatedDate { get; set; }

    [JsonIgnore]
    public bool Active { get; set; }

    public void NewRegister()
    {
        CreatedDate = DateTime.Now;
        UpdatedDate = null;
        Active = true;
    }
    public void AtualizarRegistro()
    {
        UpdatedDate = DateTime.Now;
        Active = true;
    }
    public void InativarRegistro()
    {
        Active = false;
    }
}
