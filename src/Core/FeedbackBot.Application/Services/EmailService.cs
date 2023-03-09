using System.Net;
using System.Text;
using FeedbackBot.Application.Interfaces;
using FeedbackBot.Application.Models.Configs;
using FeedbackBot.Application.Models.DTOs;
using FeedbackBot.Domain.Models.Entities;
using FeedbackBot.Persistence;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using MailKit.Search;
using MailKit.Security;
using Mapster;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;
using Serilog;
using Telegram.Bot.Types;

namespace FeedbackBot.Application.Services;

public class EmailService : IEmailService
{
    private readonly EmailConfig _emailConfig;
    private readonly ApplicationDbContext _dbContext;

    public EmailService(IOptions<EmailConfig> emailConfig, ApplicationDbContext dbContext)
    {
        _emailConfig = emailConfig.Value;
        _dbContext = dbContext;
    }

    public async Task SendEmailAsync(Project context)
    {
        try
        {
            var emailMessage = new MimeMessage();

            emailMessage.From.Add(new MailboxAddress(context.FullName, _emailConfig.BotEmail));
            emailMessage.To.Add(new MailboxAddress("", context.SchoolEmail));
            emailMessage.Subject = context.Subject;
            emailMessage.Body = new TextPart(TextFormat.Text) { Text = FormatBody(context) };

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync("smtp.gmail.com", 465, SecureSocketOptions.SslOnConnect).ConfigureAwait(false);
                await client.AuthenticateAsync(new NetworkCredential(_emailConfig.BotEmail, _emailConfig.BotPassword));
                await client.SendAsync(emailMessage).ConfigureAwait(false);
                await client.DisconnectAsync(true).ConfigureAwait(false);
            }
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception.Message);
        }
    }
    
    private string FormatBody(Project context)
    {
        StringBuilder message = new();
        message.AppendLine($"Берет ли ответственность: {context.IsResponsible}");
        message.AppendLine($"Предложил инициативу: {context.FullName} {context.Grade}");
        message.AppendLine($"Участники: {context.Members}");
        message.AppendLine($"{context.Content}");
        message.AppendLine($"Обращаться на почту: {context.StudentEmail}");
        return message.ToString();
    }
}