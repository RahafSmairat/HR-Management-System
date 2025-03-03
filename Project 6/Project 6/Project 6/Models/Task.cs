using System;
using System.Collections.Generic;

namespace Project_6.Models;

public partial class Task
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public string? Status { get; set; }

    public int? EmployeeId { get; set; }

    public DateOnly? AssignedDate { get; set; }

    public DateOnly? DueDate { get; set; }

    public virtual Employee? Employee { get; set; }
}
