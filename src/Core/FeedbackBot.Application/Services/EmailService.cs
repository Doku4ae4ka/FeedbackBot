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

            emailMessage.From.Add(new MailboxAddress(context.FullName, _emailConfig.BotUsername));
            emailMessage.To.Add(new MailboxAddress("", context.SchoolEmail));
            emailMessage.Subject = context.Subject;
            emailMessage.Body = new TextPart(TextFormat.Text) { Text = FormatBody(context) };

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync("smtp.gmail.com", 465, SecureSocketOptions.SslOnConnect).ConfigureAwait(false);
                await client.AuthenticateAsync(new NetworkCredential(_emailConfig.BotUsername, _emailConfig.BotPassword));
                await client.SendAsync(emailMessage).ConfigureAwait(false);
                await client.DisconnectAsync(true).ConfigureAwait(false);
            }
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception.Message);
        }
    }

    public async Task ReceiveEmailAsync(Project context)
    {
        try
        {
            using (var client = new ImapClient())
            {
                await client.ConnectAsync("imap.gmail.com", 993, SecureSocketOptions.SslOnConnect).ConfigureAwait(false);
                await client.AuthenticateAsync(new NetworkCredential(_emailConfig.BotUsername, _emailConfig.BotPassword)); 
                client.Inbox.Open(FolderAccess.ReadOnly);

                var uids = client.Inbox.Search(SearchQuery.SubjectContains($"Re:{context.Subject}"));

                var messages = client.Inbox.Fetch(uids, MessageSummaryItems.Envelope | MessageSummaryItems.BodyStructure);
                if (messages != null && messages.Count == 1)
                {
                    var responseDto = new EmailResponseDto();
                    //responseDto.Body = messages[0].Envelope.InReplyTo;
                    responseDto.Body = messages[0].TextBody.ToString();
                    responseDto.UserId = context.UserId;
                    var response = responseDto.Adapt<EmailResponse>();
        
                    await _dbContext.EmailResponses.AddAsync(response);
                    await _dbContext.SaveChangesAsync();
                }
                client.Disconnect (true);
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
        message.AppendLine($"Предложил инициативу: {context.FullName} {context.Grade}");
        message.AppendLine($"Участники: {context.Members}");
        message.AppendLine($"{context.Content}");
        return message.ToString();
    }
}