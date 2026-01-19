# JobEmailSender

A .NET 8 Console application that sends personalized emails to recruiters via Gmail SMTP with PDF attachment support.

## Features

- Sends personalized emails using a template
- Replaces `[Recruiter First Name]` placeholder with recipient's name
- Attaches PDF resume to each email
- Reads leads from CSV file (name, email format)
- Uses Gmail SMTP with App Password authentication
- Logs success/failure for each email sent

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Gmail account with 2-Step Verification enabled
- Gmail App Password

## Setup

1. **Clone the repository**
   ```bash
   git clone https://github.com/dawoodali09/JobEmailSender.git
   cd JobEmailSender
   ```

2. **Create appsettings.json** (copy from example)
   ```bash
   cp appsettings.example.json appsettings.json
   ```

3. **Configure Gmail credentials** in `appsettings.json`:
   ```json
   {
     "Gmail": {
       "Email": "your-email@gmail.com",
       "AppPassword": "your-16-char-app-password"
     }
   }
   ```

4. **Generate a Gmail App Password**:
   - Go to [Google Account Security](https://myaccount.google.com/security)
   - Enable 2-Step Verification (if not already enabled)
   - Go to [App Passwords](https://myaccount.google.com/apppasswords)
   - Select "Mail" and generate a new app password
   - Copy the 16-character password to `appsettings.json`

5. **Prepare your files** in the parent directory:
   - `leads.txt` - CSV file with leads (format: `Name,email@example.com`)
   - `Template.txt` - Email template with `[Recruiter First Name]` placeholder
   - `Dawood Ali.pdf` - PDF attachment (or modify code for your filename)

## Usage

```bash
dotnet run
```

The application will:
1. Load Gmail credentials from `appsettings.json`
2. Read leads from `../leads.txt`
3. Read email template from `../Template.txt`
4. Send personalized emails with PDF attachment to each lead
5. Log results to console

## File Structure

```
Parent Directory/
├── leads.txt              # CSV: Name,email
├── Template.txt           # Email template
├── Dawood Ali.pdf         # PDF attachment
└── JobEmailSender/
    ├── Program.cs
    ├── JobEmailSender.csproj
    ├── appsettings.json        # Your credentials (gitignored)
    └── appsettings.example.json
```

## leads.txt Format

```
John Smith,john.smith@company.com
Jane Doe,jane.doe@recruiting.com
```

## Template.txt Format

The template can include `[Recruiter First Name]` which will be replaced with each lead's name.

Optionally include a subject line:
```
Subject: Your Custom Subject Line

Dear [Recruiter First Name],

Your email body here...
```

If no subject line is specified, a default subject will be used.

## License

MIT
