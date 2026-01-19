using Microsoft.Extensions.Configuration;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;

class Program
{
    private const string DefaultSubject = "Senior Solution Architect (MBA) - Workday Expert | Immediate Availability | Q1/Q2 2026";

    static async Task Main(string[] args)
    {
        Console.WriteLine("=== Job Email Sender ===\n");

        // Load configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .Build();

        var gmailEmail = configuration["Gmail:Email"];
        var gmailAppPassword = configuration["Gmail:AppPassword"];

        if (string.IsNullOrEmpty(gmailEmail) || string.IsNullOrEmpty(gmailAppPassword))
        {
            Console.WriteLine("ERROR: Gmail credentials not configured in appsettings.json");
            Console.WriteLine("Please set Gmail:Email and Gmail:AppPassword");
            return;
        }

        // Get paths relative to project directory (parent of bin folder)
        var projectDir = Directory.GetParent(AppContext.BaseDirectory)?.Parent?.Parent?.Parent?.FullName
            ?? AppContext.BaseDirectory;
        var parentDir = Directory.GetParent(projectDir)?.FullName ?? projectDir;

        var leadsPath = Path.Combine(parentDir, "leads.txt");
        var templatePath = Path.Combine(parentDir, "Template.txt");
        var pdfPath = Path.Combine(parentDir, "Dawood Ali.pdf");

        // Verify required files exist
        if (!File.Exists(leadsPath))
        {
            Console.WriteLine($"ERROR: leads.txt not found at: {leadsPath}");
            return;
        }

        if (!File.Exists(templatePath))
        {
            Console.WriteLine($"ERROR: Template.txt not found at: {templatePath}");
            return;
        }

        if (!File.Exists(pdfPath))
        {
            Console.WriteLine($"ERROR: Dawood Ali.pdf not found at: {pdfPath}");
            return;
        }

        // Read template and extract subject if present
        var templateContent = await File.ReadAllTextAsync(templatePath);
        var subject = DefaultSubject;
        var body = templateContent;

        // Check if template has a Subject: line
        var lines = templateContent.Split('\n');
        foreach (var line in lines)
        {
            if (line.TrimStart().StartsWith("Subject:", StringComparison.OrdinalIgnoreCase))
            {
                subject = line.Substring(line.IndexOf(':') + 1).Trim();
                body = templateContent.Replace(line + "\n", "").Replace(line, "");
                break;
            }
        }

        // Read leads
        var leadsLines = await File.ReadAllLinesAsync(leadsPath);
        var leads = new List<(string Name, string Email)>();

        foreach (var line in leadsLines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            var parts = line.Split(',');
            if (parts.Length >= 2)
            {
                var name = parts[0].Trim();
                var email = parts[1].Trim();
                leads.Add((name, email));
            }
        }

        if (leads.Count == 0)
        {
            Console.WriteLine("ERROR: No valid leads found in leads.txt");
            return;
        }

        Console.WriteLine($"Found {leads.Count} lead(s) to process");
        Console.WriteLine($"Template loaded ({body.Length} characters)");
        Console.WriteLine($"Subject: {subject}");
        Console.WriteLine($"Attachment: {Path.GetFileName(pdfPath)}");
        Console.WriteLine();

        // Process each lead
        var successCount = 0;
        var failCount = 0;

        foreach (var (name, email) in leads)
        {
            Console.Write($"Sending to {name} <{email}>... ");

            try
            {
                // Personalize the body
                var personalizedBody = body.Replace("[Recruiter First Name]", name);

                // Create the email message
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Dawood Ali", gmailEmail));
                message.To.Add(new MailboxAddress(name, email));
                message.Subject = subject;

                // Build the message body with attachment
                var builder = new BodyBuilder();
                builder.TextBody = personalizedBody;
                builder.Attachments.Add(pdfPath);

                message.Body = builder.ToMessageBody();

                // Send via Gmail SMTP
                using var client = new SmtpClient();
                await client.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(gmailEmail, gmailAppPassword);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                Console.WriteLine("SUCCESS");
                successCount++;

                // Small delay to avoid rate limiting
                await Task.Delay(1000);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FAILED - {ex.Message}");
                failCount++;
            }
        }

        Console.WriteLine();
        Console.WriteLine($"=== Complete ===");
        Console.WriteLine($"Sent: {successCount}, Failed: {failCount}");
    }
}
