using System;
using System.Collections.Generic;

namespace Incoming.Models;

public partial class Finishedgood
{
    public int Id { get; set; }

    public string? Program { get; set; }

    public string? Partnumber { get; set; }

    public string? Base { get; set; }

    public string? Heater { get; set; }

    public string? Ntc { get; set; }

    public string? Inside { get; set; }

    public string? Outside { get; set; }

    public string? Guard { get; set; }

    public DateTime? RejectionDate { get; set; }
}
