﻿using Sabatex.Core;
using Sabatex.ObjectsExchange.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ObjectsExchange.Client.Models;
/// <summary>
/// Client node definition
/// </summary>
public class ClientNode : ClientNodeBase
{

    [MaxLength(100)]
    public string NormalizedName { get; set; } = default!;

    [Display(Name = "Password")]
    public string Password { get; set; } = default!;
    public Client? Client { get; set; }
    public Guid ClientId { get; set; }
    [NotMapped]
    public int DataCount { get; set; }
    public IEnumerable<ObjectExchange>? Objects { get; set; }
    public IEnumerable<QueryObject>? QueryObjects { get; set; }
}
