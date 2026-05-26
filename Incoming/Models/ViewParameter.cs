using System;
using System.Collections.Generic;

namespace Incoming.Models;

public partial class ViewParameter
{
    public string ProgramName { get; set; } = null!;

    public string Test { get; set; } = null!;

    public decimal Minimum { get; set; }

    public decimal Maximum { get; set; }
}
