using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;

namespace CybersecurityChatbot
{
    
    // Delegate to dynamically handle structured conversation responses
    public delegate string BotResponseDelegate(string userInput);

    public class ChatbotEngine
    {
        // Connection string pointing to your local SQL Server database instance
        private readonly string _connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=CybersecurityChatbotDB;Integrated Security=True;";

        // Data Structures for Code Optimization
        private readonly List<string> _phishingPool;
        private readonly List<string> _activityLog; // Internal storage for activity tracking
        private readonly Random _randomizer;
        private object input;
        private string? sentimentPrefix;
        private bool isTopicContinuation;

        public string? UserName { get; private set; }
        public string? FavoriteTopic { get; private set; }
        public string LastDiscussedTopic { get; private set; } = "general";

        public ChatbotEngine()
        {
            _randomizer = new Random();
            _activityLog = new List<string>();

            _phishingPool = new List<string>
            {
                "Phishing is when attackers send fake emails to steal your bank login. Always check the sender's address!",
                "Be cautious of emails asking for personal information. Scammers often disguise themselves as trusted organisations.",
                "Never click on unexpected links. Navigate to the official website directly through your browser instead."
            };

            // Log initialization tracking state
            LogAction("Chatbot application initialized successfully.");
        }

        // Public helper method to allow external modules (like a Quiz Form) to record events
        public void LogAction(string actionDescription)
        {
            string query = "INSERT INTO ActivityLogs (ActionDescription) VALUES (@ActionDesc);";

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ActionDesc", actionDescription);
                        connection.Open();
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Database Log Error: {ex.Message}");
            }
        }
        public string ProcessInput(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return "I didn't catch that. Could you please type something?";
            }

            string cleanInput = input.Trim().ToLower();

            LogAction($"You: {input}");

            // TRAFFIC INTERCEPTOR: Activity Log Commands
            if (cleanInput.Contains("show activity log") || cleanInput.Contains("what have you done for me"))
            {
                return GenerateActivityLogOutput();
            }

            // Handling name declaration setup
            if (cleanInput.StartsWith("my name is "))
            {
                UserName = input.Substring(11).Trim();
                LogAction($"User profile established for identity: '{UserName}'.");
                return $"Nice to meet you, {UserName}! What cybersecurity topics would you like to learn about today?";
            }

            // Sentiment Evaluation Check
            string sentimentPrefix = DetectSentiment(cleanInput, out bool isTopicContinuation);
            if (isTopicContinuation && !string.IsNullOrEmpty(sentimentPrefix))
            {
                LogAction($"NLP Interaction: Captured sentiment pattern and provided localized empathy fallback.");
            }

            // Context Tracking Continuity Logic
            if (cleanInput.Contains("tell me more") || cleanInput.Contains("explain more") || cleanInput.Contains("give me another tip"))
            {
                LogAction($"Context Tracking: Requested follow-up depth for topic area '{LastDiscussedTopic}'.");
                return GetResponseForKeyword(LastDiscussedTopic);
            }

            // Route user input matching keyword logic parameters
            string response = MatchKeywords(cleanInput);

            if (response != null)
            {
                // Combining sentiment context with the matched keyword response
                if (isTopicContinuation && !string.IsNullOrEmpty(sentimentPrefix))
                {
                    return $"{sentimentPrefix} {response}";
                }
                return response;
            }

            LogAction($"Fallback Triggered: Unrecognized user prompt string: '{input}'.");
            
            // Standard fallback response for unhandled topics
            return "I didn't quite understand that. Could you rephrase? I can talk about passwords, phishing, or my purpose.";
        }

        private string MatchKeywords(string cleanInput)
        {
            // Purpose
            if (cleanInput.Contains("purpose"))
            {
                LastDiscussedTopic = "purpose";
                LogAction("NLP Interaction: Custom response recognized through keyword detection ['purpose'].");
                return "My purpose is to educate citizens about phishing, passwords, and safe browsing.";
            }

            // Passwords
            if (cleanInput.Contains("password"))
            {
                LastDiscussedTopic = "password";
                LogAction("NLP Interaction: Custom response recognized through keyword detection ['password'].");
                string personalization = UserName != null ? $"{UserName}, take note: " : "";
                return personalization + "Think of a password as the key to your digital home. If it's short or easy to guess (like your birthday), " +
                       "it's like leaving your key under the doormat where any thief can find it.\n\n" +
                       "🛡️ TIP: Use at least 12 characters with a mix of symbols, numbers, and cases!";
            }

            // Phishing
            if (cleanInput.Contains("phishing"))
            {
                LastDiscussedTopic = "phishing";
                LogAction("NLP Interaction: Custom response recognized through keyword detection ['phishing'].");
                int index = _randomizer.Next(_phishingPool.Count);
                return _phishingPool[index];

                string finalResponse = sentimentPrefix + " " + _phishingPool[index];

                LogAction($"Bot: {finalResponse}");

                return finalResponse;
            }

            // Social Media
            if (cleanInput.Contains("facebook") || cleanInput.Contains("instagram") || cleanInput.Contains("social media"))
            {
                LastDiscussedTopic = "social media";
                if (FavoriteTopic == null) FavoriteTopic = "social media";
                LogAction("NLP Interaction: Custom response recognized through keyword detection ['social media']. Saved as user preference.");

                return "Social media is like a public notice board. Scammers 'harvest' your posts to find out your location, " +
                       "your high school, or your pet's name to bypass your security questions.\n\n" +
                       "🔒 SOCIAL MEDIA TIP: Set your profiles to 'Private.' Scammers use public info like your birthday or pet's name to guess your security questions.";
            }

            // Banking & EFTs
            if (cleanInput.Contains("bank") || cleanInput.Contains("eft") || cleanInput.Contains("money"))
            {
                LastDiscussedTopic = "bank";
                LogAction("NLP Interaction: Custom response recognized through keyword detection ['bank/eft/money'].");
                return "Digital banking fraud often uses 'social engineering' to create panic. " +
                       "They make you believe there is an emergency so you'll give up your One-Time Pin (OTP) without thinking.\n\n" +
                       "⚠️ BANKING ALERT: Never share your OTP (One-Time Pin) with anyone, even if they claim to be from your bank. Banks will NEVER ask for this.";
            }

            // Software Updates
            if (cleanInput.Contains("update") || cleanInput.Contains("software"))
            {
                LastDiscussedTopic = "update";
                LogAction("NLP Interaction: Custom response recognized through keyword detection ['update'].");
                return "UPDATE TIP: Keep your phone and apps updated. These updates often include 'patches' that fix security holes hackers like to use.";
            }

            // Public Wi-Fi Networks
            if (cleanInput.Contains("wifi") || cleanInput.Contains("airport") || cleanInput.Contains("coffee shop"))
            {
                LastDiscussedTopic = "wifi";
                LogAction("NLP Interaction: Custom response recognized through keyword detection ['wifi'].");
                return "Public Wi-Fi is an open airwave. Hackers can sit in the same coffee shop and use 'sniffing' tools to see exactly what you are typing on unencrypted networks.\n\n" +
                       "🌐 WI-FI TIP: Avoid logging into banking apps on public Wi-Fi. If you must use it, use a VPN to encrypt your connection.";
            }

            // Malware & Viruses
            if (cleanInput.Contains("virus") || cleanInput.Contains("malware"))
            {
                LastDiscussedTopic = "malware";
                LogAction("NLP Interaction: Custom response recognized through keyword detection ['virus/malware'].");
                return "A computer virus or malware is like a digital parasite. It attaches itself to a healthy file or app and, once opened, " +
                       "it can track your keystrokes, steal your bank details, or lock your files until you pay a ransom.\n\n" +
                       "🦠 MALWARE TIP: Only download apps from official stores like Google Play or the Apple App Store to avoid hidden 'Trojans'.";
            }

            // Exit & Program Closing
            if (cleanInput.Contains("exit") || cleanInput.Contains("bye"))
            {
                LogAction("Application exit sequence initialized via departure keywords.");
                string closureGreeting = UserName != null ? $", {UserName}" : "";

                // Shut down the WinForms thread environment
                MessageBox.Show($"Stay safe{closureGreeting}! Goodbye.", "Application Exiting", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Application.Exit();
                return "Goodbye!";
            }

            if (cleanInput.Contains("privacy"))
            {
                LastDiscussedTopic = "privacy";
                LogAction("NLP Interaction: Custom response recognized through keyword detection ['privacy'].");
                return "Privacy is your first line of defense online. Limit the amount of personal data you share publicly to minimize your digital footprint.";
            }

            // 🔍 DYNAMIC SQL KEYWORD MATCHING ENGINE
            string sqlResponse = MatchKeywordsFromDatabase(cleanInput);

            if (sqlResponse != null)
            {
                if (isTopicContinuation && !string.IsNullOrEmpty(sentimentPrefix))
                {
                    return $"{sentimentPrefix} {sqlResponse}";
                }
                return sqlResponse;
            }

            LogAction($"Fallback Triggered: Unrecognized user prompt string: '{input}'.");
            return "I didn't quite understand that. Could you rephrase? I can talk about passwords, phishing, or my purpose. You can also click 'View Activity Log' to pull database log entries!";
        }

        // 🧠 Queries the database table dynamically for registered answer keywords
        private string MatchKeywordsFromDatabase(string cleanInput)
        {
            string query = "SELECT Keyword, ResponseText FROM BotResponses;";

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        connection.Open();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string keyword = reader["Keyword"].ToString()!.ToLower();
                                if (cleanInput.Contains(keyword))
                                {
                                    LastDiscussedTopic = keyword;
                                    string responseText = reader["ResponseText"].ToString()!;
                                    
                                    LogAction($"SQL Interaction: Successfully pulled content match from [BotResponses] for keyword ['{keyword}'].");
                                    return responseText;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return $"[Database Error]: Failed to retrieve responses. Details: {ex.Message}";
            }

            return null!;
        }

        public string GenerateActivityLogOutput()
        {
            if (_activityLog.Count == 0)
            {
                return "No activities have been recorded in the current session log yet.";
            }

            StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine("Here's a summary of recent actions:");

            // Fetch either the last 8 entries or the maximum available entries to keep output clean and readable
            int itemsToDisplay = Math.Min(_activityLog.Count, 8);
            int startPoint = _activityLog.Count - itemsToDisplay;

            int counter = 1;
            for (int i = startPoint; i < _activityLog.Count; i++)
            {
                sb.AppendLine($"{counter}. {_activityLog[i]}");
                counter++;
            }

            if (_activityLog.Count > 8)
            {
                sb.AppendLine($"\n🔄 ... showing latest {itemsToDisplay} actions. (Total History: {_activityLog.Count} records).");
            }

            return sb.ToString();
        }

        private string GetResponseForKeyword(string topic)
        {
            return topic switch
            {
                "password" => "Another tip for passwords: Use a password manager to securely store unique credentials for every single account.",
                "phishing" => "Additional Phishing Warning: Check links before clicking by hovering your mouse over them to verify the destination URL.",
                "social media" => "Remember, don't accept friend requests from individuals you don't know personally in the physical world.",
                "bank" => "Always log into your banking platforms manually via an official mobile app or verified web bookmark—never via links inside SMS alerts.",
                "wifi" => "When out and about, turn off automatic Wi-Fi joining options on your devices to ensure they don't latch onto rogue honeypots.",
                "malware" => "Make sure to run a reputable background anti-malware service scan at least once weekly to inspect temporary system caches.",
                _ => "What other cybersecurity fields would you like me to clarify for you?"
            };
        }

        private string DetectSentiment(string input, out bool isTopicContinuation)
        {
            isTopicContinuation = false;

            if (input.Contains("worried") || input.Contains("scared") || input.Contains("afraid"))
            {
                isTopicContinuation = true;
                return "It's completely understandable to feel that way. Scammers can be very convincing. Let me share some tips to help you stay safe.";
            }
            if (input.Contains("frustrated") || input.Contains("annoyed") || input.Contains("hate"))
            {
                isTopicContinuation = true;
                return "Dealing with technology risks can be incredibly annoying. Let's work through this together.";
            }
            if (input.Contains("curious") || input.Contains("interested"))
            {
                return "That's awesome! Gaining security awareness keeps you a step ahead of bad actors. ";
            }

            return string.Empty;
        }

        // Fetching the conversation text logs straight out of the SQL Database
        public string GenerateChatHistoryOutput()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Complete Conversation Chat History (SQL Database Records):");
            sb.AppendLine("--------------------------------------------------");

            // Query to pull recent session traffic elements
            string query = "SELECT TOP 30 Timestamp, ActionDescription FROM ActivityLogs ORDER BY Timestamp DESC;";

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        connection.Open();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            List<string> dialogueLines = new List<string>();

                            while (reader.Read())
                            {
                                string desc = reader["ActionDescription"].ToString()!;

                                // We filter for conversation indicators or display the log entries cleanly
                                if (desc.StartsWith("User profile") || desc.Contains("SQL Interaction") || desc.Contains("Fallback Triggered") || desc.Contains("NLP Interaction"))
                                {
                                    // Optional: Skip background system processing milestones if you only want text lines
                                    continue;
                                }

                                string timeStr = Convert.ToDateTime(reader["Timestamp"]).ToString("HH:mm:ss");
                                dialogueLines.Add($"[{timeStr}] {desc}");
                            }

                            if (dialogueLines.Count == 0)
                            {
                                return "No dialogue records found in the database. Start chatting with the bot first!";
                            }

                            // Display chronologically (oldest at the top to newest at the bottom)
                            for (int i = dialogueLines.Count - 1; i >= 0; i--)
                            {
                                sb.AppendLine(dialogueLines[i]);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return $"Failed to pull chat logs from SQL database. Error: {ex.Message}";
            }

            return sb.ToString();
        }
    }
}