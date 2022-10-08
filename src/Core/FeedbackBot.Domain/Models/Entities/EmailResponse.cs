using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeedbackBot.Domain.Models.Entities;

public class EmailResponse
{
    public Guid Id { get; set; }
    public string ResponseSubject { get; set; } = null!;
    public string ResponseContent { get; set; } = null!;
}
