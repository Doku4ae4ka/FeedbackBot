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

    /*public async Task StartReceivingEmailsAsync()
    {

		var logger = new ProtocolLogger(Console.OpenStandardError());

	    using (var client = new ImapClient(logger))
	    {
		    var credentials = new NetworkCredential(_emailConfig.BotUsername, _emailConfig.BotPassword);
		    var uri = new Uri("imaps://imap.gmail.com");

		    await client.ConnectAsync(uri);

		    // Remove the XOAUTH2 authentication mechanism since we don't have an OAuth2 token.
		    client.AuthenticationMechanisms.Remove("XOAUTH2");

		    await client.AuthenticateAsync(credentials);

		    await client.Inbox.OpenAsync(FolderAccess.ReadOnly);

		    // keep track of the messages
		    IList<IMessageSummary> messages = null!;
		    int count = 0;

		    if (client.Inbox.Count > 0)
		    {
			    messages = client.Inbox.Fetch(0, -1, MessageSummaryItems.Full | MessageSummaryItems.UniqueId).ToList();
			    count = messages.Count;
		    }

		    // connect to some events...
		    client.Inbox.CountChanged += (sender, e) =>
		    {
			    // Note: the CountChanged event can fire for one of two reasons:
			    //
			    // 1. New messages have arrived in the folder.
			    // 2. Messages have been expunged from the folder.
			    //
			    // If messages have been expunged, then the MessageExpunged event
			    // should also fire and it should fire *before* the CountChanged
			    // event fires.
			    var folder = (ImapFolder)sender;

			    if (folder.Count > count)
			    {
				    // New messages have arrived in the folder.
				    Log.Debug("{0}: {1} new messages have arrived.", folder, folder.Count - count);

				    // Note: your first instict may be to fetch these new messages now, but you cannot do
				    // that in an event handler (the ImapFolder is not re-entrant).
			    }
			    else if (folder.Count < count)
			    {
				    // Note: this shouldn't happen since we are decrementing count in the MessageExpunged handler.
				    Log.Debug("{0}: {1} messages have been removed.", folder, count - folder.Count);
			    }
			    else
			    {
				    // We just got a CountChanged event after 1 or more MessageExpunged events.
				    Log.Debug("{0}: the message count is now {1}.", folder, folder.Count);
			    }

			    // update our count so we can keep track of whether or not CountChanged events
			    // signify new mail arriving.
			    count = folder.Count;
		    };

		    client.Inbox.MessageExpunged += (sender, e) =>
		    {
			    var folder = (ImapFolder)sender;

			    if (e.Index < messages.Count)
			    {
				    var message = messages[e.Index];

				    Log.Debug("{0}: expunged message {1}: Subject: {2}", folder, e.Index,
					    message.Envelope.Subject);

				    // Note: If you are keeping a local cache of message information
				    // (e.g. MessageSummary data) for the folder, then you'll need
				    // to remove the message at e.Index.
				    messages.RemoveAt(e.Index);
			    }
			    else
			    {
				    Log.Debug("{0}: expunged message {1}: Unknown message.", folder, e.Index);
			    }

			    // update our count so we can keep track of whether or not CountChanged events
			    // signify new mail arriving.
			    count--;
		    };

		    /*client.Inbox.MessageFlagsChanged += (sender, e) =>
		    {
			    var folder = (ImapFolder)sender;

			    Log.Debug("{0}: flags for message {1} have changed to: {2}.", folder, e.Index, e.Flags);
		    };

		    Console.WriteLine("Hit any key to end the IDLE command.");
		    using (var done = new CancellationTokenSource())
		    {
			    var task = client.IdleAsync(done.Token);

			    Console.ReadKey();
			    done.Cancel();
			    task.Wait();
		    }

		    if (count > messages.Count)
		    {
			    Log.Debug("The new messages that arrived during IDLE are:");
			    foreach (var message in client.Inbox.Fetch(messages.Count, -1,
				             MessageSummaryItems.Full | MessageSummaryItems.UniqueId))
				    Log.Debug("Subject: {0}", message.Envelope.Subject);
		    }#1#

		    //client.Disconnect(true);
	    }
    }*/

    private string FormatBody(Project context)
    {
        StringBuilder message = new();
        message.AppendLine($"Предложил инициативу: {context.FullName} {context.Grade}");
        message.AppendLine($"Участники: {context.Members}");
        message.AppendLine($"{context.Content}");
        message.AppendLine($"Обращаться на почту: {context.StudentEmail}");
        return message.ToString();
    }
}