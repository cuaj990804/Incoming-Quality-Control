using System;
using System.Collections.Generic;

namespace Incoming.Models;

public partial class Rejection
{
    public int Id { get; set; }

    public string Program { get; set; } = null!;

    public string Base { get; set; } = null!;

    public string Heater { get; set; } = null!;

    public string Ntc { get; set; } = null!;

    public string Inside { get; set; } = null!;

    public string Outside { get; set; } = null!;

    public string Guard { get; set; } = null!;

    public DateTime? RejectionDate { get; set; }

    public string? Molding { get; set; }

    public string? Gap { get; set; }

    public string? Connector { get; set; }
}
