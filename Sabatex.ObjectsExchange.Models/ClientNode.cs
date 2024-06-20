using Sabatex.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace Sabatex.ObjectsExchange.Models;

/// <summary>
/// Base class for determine client node
/// </summary>
public class ClientNode:IEntityBase<Guid>
{
    /// <summary>
    /// Client ID - UUID string
    /// </summary>
    public Guid Id { get; set; }
    //string IEntityBase<Guid>.KeyAsString() => Id.ToString();
    /// <summary>
    /// Frendly client name (not indexed)
    /// </summary>

    [MaxLength(100)]
    public string Name { get; set; } = default!;

    /// <summary>
    /// Password hash
    /// </summary>
    [Display(Name = "Password")]
    public string Password { get; set; } = default!;

    public Client? Client { get; set; }
    public Guid ClientId { get; set; }
    [NotMapped]
    public int ObjectsCount { get; set; }
    public DateTime CounterReseted { get; set; }
    [JsonIgnore]
    public IEnumerable<ObjectExchange>? Objects { get; set; }



    /// <summary>
    /// Client description
    /// </summary>
    /// 
    public string? Description { get; set; }
    /// <summary>
    /// Client list id's with delimiters char ; for access node  
    /// </summary>
    public string? ClientAccess { get; set; }



    /// <summary>
    /// Get 
    /// </summary>
    /// <returns></returns>
    public IEnumerable<Guid> GetClientAccess() => ClientAccess?.Split(',').Select(s => new Guid(s)) ?? new Guid[] { };
    /// <summary>
    /// Demo mode (limit count transactions and trolling)
    /// </summary>
    public bool IsDemo { get; set; } = true;
    /// <summary>
    /// Transactions counter
    /// </summary>
    public uint Counter { get; set; }
    /// <summary>
    /// limit transactions per day (drop every day)
    /// </summary>
    public uint MaxOperationPerDay { get; set; } = 1000;
}
