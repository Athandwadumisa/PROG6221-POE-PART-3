using System;
using System.Drawing;
using System.Media;
using System.Windows.Forms;

namespace CybersecurityChatbot
{
    public class MainForm : Form
    {
        private readonly ChatbotEngine _engine;

        // Form Component Controls
        private RichTextBox _chatHistoryDisplay = null!;
        private TextBox _userInputField = null!;
        private Button _sendTextButton = null!;
        private Button _viewLogButton = null!;
        private Panel _headerBanner = null!;
        private Label _bannerLabel = null!;

        public MainForm()
        {
            _engine = new ChatbotEngine();
            InitializeComponentLayout();
            DisplayWelcomeMessage();
        }

        private void InitializeComponentLayout()
        {
            // Visual arrangement adjustments
            this.Text = "CyberShield Awareness Assistant";
            this.Size = new Size(580, 710);
            this.MinimumSize = new Size(450, 580);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(20, 24, 33);

            // View Activity Log Dashboard Button
            _viewLogButton = new Button
            {
                Text = "View Activity Log",
                Location = new Point(445, 595), 
                Width = 105,
                Height = 28,
                BackColor = Color.FromArgb(45, 55, 72),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };
            _viewLogButton.FlatAppearance.BorderSize = 0;
            _viewLogButton.Click += ViewLogButton_Click;
            this.Controls.Add(_viewLogButton);

            // Top Banner Accent
            _headerBanner = new Panel
            {
                Dock = DockStyle.Top,
                Height = 65,
                BackColor = Color.FromArgb(26, 54, 93) 
            };

            _bannerLabel = new Label
            {
                Text = "🛡️ CyberShield Bot AI",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Location = new Point(15, 18),
                AutoSize = true
            };
            _headerBanner.Controls.Add(_bannerLabel);

            // Chat History Window Display area
            _chatHistoryDisplay = new RichTextBox
            {
                Location = new Point(15, 80),
                Width = 505,
                Height = 440,
                ReadOnly = true,
                BackColor = Color.Black,
                BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 10.5f),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            // User Inputs Field
            _userInputField = new TextBox
            {
                Location = new Point(15, 540),
                Width = 390,
                Font = new Font("Segoe UI", 11),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };
            _userInputField.KeyDown += UserInputField_KeyDown;

            // Execution Trigger Button
            _sendTextButton = new Button
            {
                Text = "Send",
                Location = new Point(415, 538),
                Width = 105,
                Height = 32,
                BackColor = Color.FromArgb(49, 130, 206), 
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };
            _sendTextButton.FlatAppearance.BorderSize = 0;
            _sendTextButton.Click += SendTextButton_Click;

            // Map variables inside control registry framework
            this.Controls.Add(_headerBanner);
            this.Controls.Add(_chatHistoryDisplay);
            this.Controls.Add(_userInputField);
            this.Controls.Add(_sendTextButton);
            this.Controls.Add(_viewLogButton);
        }

        private void DisplayWelcomeMessage()
        {
            // ASCII Banner design
            string asciiBanner =
        @"==============================================" + "\n" +
        @"          ____               ____        _   
                   / ___|   _ _ __ ___| __ )  ___ | |_ 
                  | |  | | | | '__/ _ \  _ \ / _ \| __|
                  | |__| |_| | | |  __/ |_) | (_) | |_ 
                   \____\__, |_|  \___|____/ \___/ \__|
                        |___/

                      Cyber Awareness Assistant         " + "\n" +
        @"==============================================";

            // Setting a cool terminal style color to fit the green text theme in the image
            _chatHistoryDisplay.SelectionColor = Color.FromArgb(72, 187, 120);
            _chatHistoryDisplay.AppendText(asciiBanner + Environment.NewLine + Environment.NewLine);

            string welcomeGreeting = "Hello! I am your CyberShield assistant. To build your personal profile session, what is your name?";
            AppendBotOutput(welcomeGreeting);

            // 🎙️ Trigger your precise WAV voice file greeting asset
            string audioPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "greeting.wav");
            PlayVoiceGreeting(audioPath);
        }

        private void PlayVoiceGreeting(string filePath)
        {
            try
            {
                // Playing WAV file
                SoundPlayer player = new SoundPlayer(filePath);

                player.Play();
            }
            catch (Exception)
            {
                _chatHistoryDisplay.SelectionColor = Color.Blue;
                _chatHistoryDisplay.AppendText("[An error occured while trying to play the sound, skipping voice greeting...]" + Environment.NewLine + Environment.NewLine);
            }
        }

        private void ProcessConversationTurn()
        {
            string textInput = _userInputField.Text.Trim();
            if (string.IsNullOrEmpty(textInput)) return;

            // Render Output Stream logs for users
            AppendUserText(textInput);
            _userInputField.Clear();

            // Run backend engine assessment rules
            string botResult = _engine.ProcessInput(textInput);
            AppendBotOutput(botResult);
        }

        private void AppendUserText(string message)
        {
            _chatHistoryDisplay.SelectionStart = _chatHistoryDisplay.TextLength;
            _chatHistoryDisplay.SelectionLength = 0;
            _chatHistoryDisplay.SelectionColor = Color.FromArgb(99, 179, 237); 
            _chatHistoryDisplay.SelectionFont = new Font(_chatHistoryDisplay.Font, FontStyle.Bold);
            _chatHistoryDisplay.AppendText("You: ");

            _chatHistoryDisplay.SelectionColor = Color.Yellow;
            _chatHistoryDisplay.SelectionFont = new Font(_chatHistoryDisplay.Font, FontStyle.Regular);
            _chatHistoryDisplay.AppendText(message + Environment.NewLine + Environment.NewLine);
            _chatHistoryDisplay.ScrollToCaret();
        }

        private void AppendBotOutput(string message)
        {
            _chatHistoryDisplay.SelectionStart = _chatHistoryDisplay.TextLength;
            _chatHistoryDisplay.SelectionLength = 0;
            _chatHistoryDisplay.SelectionColor = Color.FromArgb(72, 187, 120); 
            _chatHistoryDisplay.SelectionFont = new Font(_chatHistoryDisplay.Font, FontStyle.Bold);
            _chatHistoryDisplay.AppendText("Bot: ");

            _chatHistoryDisplay.SelectionColor = Color.FromArgb(226, 232, 240);
            _chatHistoryDisplay.SelectionFont = new Font(_chatHistoryDisplay.Font, FontStyle.Regular);
            _chatHistoryDisplay.AppendText(message + Environment.NewLine + Environment.NewLine);
            _chatHistoryDisplay.ScrollToCaret();
        }

        private void SendTextButton_Click(object? sender, EventArgs e)
        {
            ProcessConversationTurn();
        }

        private void ViewLogButton_Click(object? sender, EventArgs e)
        {
            string conversationHistory = _engine.GenerateChatHistoryOutput();

            MessageBox.Show(
                conversationHistory,
                "Archived Chat Conversation History Logs",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }

        private void UserInputField_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true; 
                ProcessConversationTurn();
            }
        }
    }
}