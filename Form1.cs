using System;
using System.Drawing;
using System.Windows.Forms;

namespace AdBlocker
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            bool darkTheme = DarkTitleBar.IsSystemThemeDark();
            DarkTitleBar.UseImmersiveDarkMode(Handle, darkTheme);
            SquareBorder.RemoveSquare(this);
            changeBackgroundColor();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DNSChanger.RestoreDefaultDnsForAllInterfaces();

            changeBackgroundColor();
            MessageBoxHelper.ShowMessageBoxAtOwner(this,
                            "Please close and reopen your browser for the changes to take effect.",
                            "AdBlocker Disabled",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DNSChanger.SetDnsForAllInterfaces("94.140.14.14", "94.140.15.15", true);
            DNSChanger.SetDnsForAllInterfaces("2a10:50c0::ad1:ff", "2a10:50c0::ad2:ff", false);

            changeBackgroundColor();
            MessageBoxHelper.ShowMessageBoxAtOwner(this,
                            "Please close and reopen your browser for the changes to take effect.",
                            "AdBlocker Enabled",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            DNSChanger.SetDnsForAllInterfaces("1.1.1.1", "1.0.0.1", true);
            DNSChanger.SetDnsForAllInterfaces("2606:4700:4700::1111", "2606:4700:4700::1001", false);

            changeBackgroundColor();
            MessageBoxHelper.ShowMessageBoxAtOwner(this, 
                            "Please close and reopen your browser for the changes to take effect.",
                            "AdBlocker Disabled",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
        }

        private void changeBackgroundColor()
        {
            if (DNSChanger.IsDnsSetTo("94.140.14.14", "94.140.15.15")) this.BackColor = Color.Green;
            else if (DNSChanger.IsDnsSetTo("1.1.1.1", "1.0.0.1")) this.BackColor = Color.Orange;
            else this.BackColor = Color.Red;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            string title = "Thank You!";
            string message = "Thank you for using ADBlocker Enabler!\n\nCredits:\nDeveloped by @_giovannigiannone\nDNS created by https://adguard-dns.io/";
            MessageBoxHelper.ShowMessageBoxAtOwner(this, message, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void Form1_Activated(object sender, EventArgs e)
        {
            this.Opacity = 0.85; // Full opacity when focused
        }

        private void Form1_Deactivate(object sender, EventArgs e)
        {
            this.Opacity = 0.25; // Reduce opacity when unfocused
        }
    }
}
