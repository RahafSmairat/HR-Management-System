using System;
using System.Collections.Generic;

namespace Project_6.Models;

public partial class Employee
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Email { get; set; }

    public string? Password { get; set; }

    public string? ProfileImage { get; set; }

    public string? Position { get; set; }

    public int? DepartmentId { get; set; }

    public int? ManagerId { get; set; }

    public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();

    public virtual Department? Department { get; set; }

    public virtual ICollection<EmpTask> EmpTasks { get; set; } = new List<EmpTask>();

    public virtual ICollection<Evaluation> Evaluations { get; set; } = new List<Evaluation>();

    public virtual Manager? Manager { get; set; }

    public virtual ICollection<RequestLeave> RequestLeaves { get; set; } = new List<RequestLeave>();
}
