using System;
using System.Collections.Generic;

namespace Incoming.Models;

public partial class Parameter
{
    public int Id { get; set; }

    public int? ProgramId { get; set; }

    public string Test { get; set; } = null!;

    public decimal Minimum { get; set; }

    public decimal Maximum { get; set; }

    public virtual InspectionProgram? Program { get; set; }
}
