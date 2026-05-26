using System;
using System.Collections.Generic;

namespace Incoming.Models;

public partial class InspectionProgram
{
    public int Id { get; set; }

    public string ProgramName { get; set; } = null!;

    public virtual ICollection<Parameter> Parameters { get; set; } = new List<Parameter>();
}
