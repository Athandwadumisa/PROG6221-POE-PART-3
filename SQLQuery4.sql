CREATE DATABASE CybersecurityChatbotDB;
GO

USE CybersecurityChatbotDB;
GO

CREATE TABLE BotResponses (
    ResponseID INT IDENTITY(1,1) PRIMARY KEY,
    Keyword VARCHAR(50) UNIQUE NOT NULL,
    ResponseText NVARCHAR(MAX) NOT NULL
);
GO

CREATE TABLE ActivityLogs (
    LogID INT IDENTITY(1,1) PRIMARY KEY,
    Timestamp DATETIME DEFAULT GETDATE(),
    ActionDescription NVARCHAR(MAX) NOT NULL
);
GO

INSERT INTO BotResponses (Keyword, ResponseText) VALUES
('purpose', 'My purpose is to educate citizens about phishing, passwords, and safe browsing.'),
('password', 'Think of a password as the key to your digital home. If it''s short or easy to guess, it''s like leaving your key under the doormat. 🛡️ TIP: Use at least 12 characters with a mix of symbols, numbers, and cases!'),
('bank', 'Digital banking fraud often uses ''social engineering'' to create panic. ⚠️ BANKING ALERT: Never share your OTP with anyone, even if they claim to be from your bank. Banks will NEVER ask for this.'),
('wifi', 'Public Wi-Fi is an open airwave. Hackers can use ''sniffing'' tools to see what you type. 🌐 WI-FI TIP: Avoid logging into banking apps on public Wi-Fi. Use a VPN to encrypt your connection.'),
('malware', 'A computer virus or malware is like a digital parasite. 🦠 MALWARE TIP: Only download apps from official stores like Google Play or the Apple App Store to avoid hidden ''Trojans''.'),
('privacy', 'Privacy is your first line of defense online. Limit the amount of personal data you share publicly to minimize your digital footprint.');
GO