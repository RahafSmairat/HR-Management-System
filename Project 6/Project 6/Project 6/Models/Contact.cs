using System;
using System.Collections.Generic;

namespace Project_6.Models;

public partial class Contact
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Email { get; set; }

    public string? Subject { get; set; }

    public string? Message { get; set; }

    public DateOnly? SubmissionDate { get; set; }
}
