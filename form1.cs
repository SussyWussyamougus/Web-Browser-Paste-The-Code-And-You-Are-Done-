using System;
using System.Drawing;
using System.Windows.Forms;
using CefSharp;
using CefSharp.WinForms;

namespace WindowsFormsApp1
{
    // The Program class contains the main entry point for the application.
    // The 'partial' keyword is necessary to allow the class definition
    // to be split across multiple files in the same project.
    public partial class Program
    {
        [STAThread]
        public static void Main()
        {
            // Initialize CefSharp, this needs to be done once per application.
            CefSettings settings = new CefSettings();
            Cef.Initialize(settings);

            // Enable visual styles and text rendering compatible with the rest of the application.
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Run the main form.
            Application.Run(new Form1());
        }
    }

    // This is the Form1 class, which defines the browser's UI.
    // The 'partial' keyword is often used when a form has a designer file,
    // but in this case, the form is built entirely with code.
    public partial class Form1 : Form
    {
        private ChromiumWebBrowser browser;
        private ToolStrip toolStrip;
        private ToolStripButton backButton;
        private ToolStripButton forwardButton;
        private ToolStripButton homeButton;
        private ToolStripButton refreshButton;
        private ToolStripButton stopButton;
        private ToolStripTextBox addressTextBox;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel toolStripStatusLabel;

        public Form1()
        {
            // The UI is built programmatically within this method.
            SetupBrowserUI();

            // Set the form's initial size and state.
            this.Text = "C# Browser";
            this.WindowState = FormWindowState.Maximized;
        }

        private void SetupBrowserUI()
        {
            // 1. Create the ToolStrip for the top bar.
            this.toolStrip = new ToolStrip();
            this.toolStrip.Dock = DockStyle.Top;
            this.toolStrip.GripStyle = ToolStripGripStyle.Hidden;
            this.toolStrip.BackColor = Color.LightGray;

            // 2. Create the navigation buttons.
            this.backButton = new ToolStripButton("Back");
            this.backButton.Image = SystemIcons.WinLogo.ToBitmap(); // Placeholder for a nice icon.
            this.backButton.Click += (sender, e) => browser.Back();
            this.backButton.Enabled = false; // Initially disabled.

            this.forwardButton = new ToolStripButton("Forward");
            this.forwardButton.Image = SystemIcons.Warning.ToBitmap(); // Placeholder
            this.forwardButton.Click += (sender, e) => browser.Forward();
            this.forwardButton.Enabled = false; // Initially disabled.

            this.refreshButton = new ToolStripButton("Refresh");
            this.refreshButton.Image = SystemIcons.Information.ToBitmap(); // Placeholder
            this.refreshButton.Click += (sender, e) => browser.Reload();
            this.refreshButton.Enabled = false; // Initially disabled.

            this.homeButton = new ToolStripButton("Home");
            this.homeButton.Image = SystemIcons.Application.ToBitmap(); // Placeholder
            this.homeButton.Click += (sender, e) => browser.Load("https://www.google.com");

            this.stopButton = new ToolStripButton("Stop");
            this.stopButton.Image = SystemIcons.Error.ToBitmap(); // Placeholder
            this.stopButton.Click += (sender, e) => browser.Stop();
            this.stopButton.Enabled = false; // Initially disabled.

            // 3. Create the address bar.
            this.addressTextBox = new ToolStripTextBox();
            this.addressTextBox.Name = "addressTextBox";
            // The 'Spring' property does not exist for ToolStripTextBox.
            // We will handle resizing manually in the OnResize event.
            this.addressTextBox.KeyDown += AddressTextBox_KeyDown;

            // 4. Add controls to the ToolStrip.
            this.toolStrip.Items.AddRange(new ToolStripItem[] {
                this.backButton,
                this.forwardButton,
                this.refreshButton,
                this.homeButton,
                this.stopButton,
                new ToolStripSeparator(),
                this.addressTextBox
            });

            // 5. Create the StatusStrip for the bottom bar.
            this.statusStrip = new StatusStrip();
            this.statusStrip.Dock = DockStyle.Bottom;
            this.statusStrip.SizingGrip = false;

            // 6. Create the status label.
            this.toolStripStatusLabel = new ToolStripStatusLabel();

            // 7. Add controls to the StatusStrip.
            this.statusStrip.Items.AddRange(new ToolStripItem[] {
                this.toolStripStatusLabel
            });

            // 8. Create the ChromiumWebBrowser control.
            this.browser = new ChromiumWebBrowser("https://www.google.com");
            this.browser.Dock = DockStyle.Fill;

            // 9. Add event handlers for browser events.
            this.browser.LoadingStateChanged += OnLoadingStateChanged;
            this.browser.TitleChanged += OnTitleChanged;
            this.browser.AddressChanged += OnAddressChanged;
            this.browser.StatusMessage += OnStatusMessageChanged;

            // 10. Add all controls to the form in the correct order.
            this.Controls.Add(this.browser);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.toolStrip);
        }

        // --- Event Handlers ---

        private void AddressTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string url = addressTextBox.Text;

                // Check if the input is a valid URL.
                if (Uri.IsWellFormedUriString(url, UriKind.Absolute))
                {
                    browser.Load(url);
                }
                else
                {
                    // If it's not a valid URL, treat it as a search query.
                    string searchQuery = Uri.EscapeDataString(url);
                    browser.Load($"https://www.google.com/search?q={searchQuery}");
                }

                e.SuppressKeyPress = true; // Stop the beep sound.
            }
        }

        private void OnLoadingStateChanged(object sender, LoadingStateChangedEventArgs e)
        {
            this.Invoke(new MethodInvoker(() =>
            {
                backButton.Enabled = e.CanGoBack;
                forwardButton.Enabled = e.CanGoForward;
                stopButton.Enabled = e.IsLoading;
                refreshButton.Enabled = !e.IsLoading;

                if (!e.IsLoading)
                {
                    toolStripStatusLabel.Text = "Done";
                }
                else
                {
                    toolStripStatusLabel.Text = "Loading...";
                }
            }));
        }

        private void OnTitleChanged(object sender, TitleChangedEventArgs e)
        {
            this.Invoke(new MethodInvoker(() => this.Text = e.Title));
        }

        private void OnAddressChanged(object sender, AddressChangedEventArgs e)
        {
            this.Invoke(new MethodInvoker(() => addressTextBox.Text = e.Address));
        }

        private void OnStatusMessageChanged(object sender, StatusMessageEventArgs e)
        {
            this.Invoke(new MethodInvoker(() =>
            {
                if (!string.IsNullOrEmpty(e.Value))
                {
                    toolStripStatusLabel.Text = e.Value;
                }
                else
                {
                    // Reset status label when the message is empty (e.g., mouse leaves link).
                    toolStripStatusLabel.Text = browser.IsLoading ? "Loading..." : "Done";
                }
            }));
        }

        protected override void OnResize(EventArgs e)
        {
            // Dynamically resize the address bar to fill the available space.
            base.OnResize(e);
            if (toolStrip != null && addressTextBox != null)
            {
                int totalWidth = toolStrip.Width;
                int buttonWidths = backButton.Width + forwardButton.Width + refreshButton.Width + homeButton.Width + stopButton.Width;
                // Add a small buffer for the separator and padding.
                int buffer = 30;
                int newWidth = totalWidth - buttonWidths - buffer;
                if (newWidth > 100)
                {
                    addressTextBox.Width = newWidth;
                }
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            // Clean up CefSharp resources when the form is closed.
            if (!browser.IsDisposed)
            {
                browser.Dispose();
            }
            Cef.Shutdown();
            base.OnClosed(e);
        }
    }
}
