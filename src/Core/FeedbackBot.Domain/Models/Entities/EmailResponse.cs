using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeedbackBot.Domain.Models.Entities;

public class EmailResponse
{
    public Guid Id { get; set; }
    public long UserId { get; set; }
    public string Body { get; set; } = null!;
}
