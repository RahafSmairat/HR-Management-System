using System;
using System.Collections.Generic;

namespace Project_6.Models;

public partial class RequestLeave
{
    public int Id { get; set; }

    public int? EmployeeId { get; set; }

    public DateOnly? RequestDate { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public string? RequestName { get; set; }

    public string? RequestDescription { get; set; }

    public TimeOnly? StartTime { get; set; }

    public TimeOnly? EndTime { get; set; }

    public string? Status { get; set; }

    public virtual Employee? Employee { get; set; }
}
