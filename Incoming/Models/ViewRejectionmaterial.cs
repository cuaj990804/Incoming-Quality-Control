using System;
using System.Collections.Generic;

namespace Incoming.Models;

public partial class ViewRejectionmaterial
{
    public string Category { get; set; } = null!;

    public string? Program { get; set; }

    public string? Base { get; set; }

    public string? Heater { get; set; }

    public string? Ntc { get; set; }

    public string? Inside { get; set; }

    public string? Outside { get; set; }

    public string? Guard { get; set; }

    public DateTime? RejectionDate { get; set; }

    public string? Molding { get; set; }

    public string? Gap { get; set; }

    public string? Connector { get; set; }

    public string? Partnumber { get; set; }
}
