using System;
using System.Collections.Generic;

namespace Incoming.Models;

public partial class Defect
{
    public int Id { get; set; }

    public string DefectName { get; set; } = null!;
}
