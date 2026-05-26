using System;
using System.Collections.Generic;

namespace Incoming.Models;

public partial class Container
{
    public int Id { get; set; }

    public string? Program { get; set; }

    public int Quantity { get; set; }

    public int PartialCount { get; set; }

    public string Status { get; set; } = null!;

    public DateTime DateStart { get; set; }

    public DateTime? DateEnd { get; set; }
}
