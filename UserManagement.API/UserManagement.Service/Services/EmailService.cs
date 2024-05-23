﻿using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Options;
using UserManagement.Service.Models;

namespace UserManagement.Service.Services;
public class EmailService : IEmailService
{
    private readonly EmailConfiguration _emailConfiguration;
    public EmailService(IOptions<EmailConfiguration> emailConfiguration)
    {
        _emailConfiguration = emailConfiguration.Value;
    }

    public void SendEmail(Message message)
    {
        var emailMessage = CreateEmailMessage(message);
        Send(emailMessage);
    }
    private MimeMessage CreateEmailMessage(Message message)
    {
        var emailMessage = new MimeMessage();
        emailMessage.From.Add(new MailboxAddress("email", _emailConfiguration.From));
        emailMessage.To.AddRange(message.To);
        emailMessage.Subject = message.Subject;
        emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Text) { Text = message.Content };
        return emailMessage;
    }
    private void Send(MimeMessage mailMessage)
    {
        using var client = new SmtpClient();
        try
        {
            client.Connect(_emailConfiguration.SmtpServer, _emailConfiguration.Port, MailKit.Security.SecureSocketOptions.StartTls);
            //client.Connect(_emailConfiguration.SmtpServer, _emailConfiguration.Port, true);
            client.AuthenticationMechanisms.Remove("XOAUTH2");
            client.Authenticate(_emailConfiguration.UserName, _emailConfiguration.Password);
            client.Send(mailMessage);
        }

        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        finally
        {
            client.Disconnect(true);
            client.Dispose();
        }
    }
}

