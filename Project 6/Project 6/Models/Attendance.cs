using System;
using System.Collections.Generic;

namespace Project_6.Models;

public partial class Attendance
{
    public int Id { get; set; }

    public int? EmployeeId { get; set; }

    public DateOnly? Date { get; set; }

    public TimeOnly? PunchInTime { get; set; }

    public TimeOnly? PunchOutTime { get; set; }

    public virtual Employee? Employee { get; set; }
}
