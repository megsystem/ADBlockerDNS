using System;
using System.Diagnostics;
using System.Security.Principal;
using System.Threading;
using System.Windows.Forms;

namespace AdBlocker
{
    internal static class Program
    {
        // Unique mutex name for your application. Change it to something unique.
        private const string MutexName = "###UniqueMutexADBlockerByGiovanniGiannone###";

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Enable Visual Styles
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Check if another instance is running
            using (Mutex mutex = new Mutex(true, MutexName, out bool createdNew))
            {
                if (!createdNew)
                {
                    MessageBox.Show(
                        "Another instance of the application is already running.",
                        "Instance Running",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                    return;
                }

                // Check if running as administrator
                if (!IsAdministrator())
                {
                    DialogResult result = MessageBox.Show(
                        "This program needs to be run as administrator.\nDo you want to restart with elevated privileges?",
                        "Elevation Required",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning
                    );

                    if (result == DialogResult.Yes)
                    {
                        ProcessStartInfo procInfo = new ProcessStartInfo
                        {
                            UseShellExecute = true,
                            WorkingDirectory = Environment.CurrentDirectory,
                            FileName = Application.ExecutablePath,
                            Verb = "runas" // Triggers UAC prompt for elevation
                        };

                        try
                        {
                            Process.Start(procInfo);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(
                                "Failed to restart with administrator privileges.\n" + ex.Message,
                                "Error",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error
                            );
                        }
                    }

                    // Exit the current instance
                    Environment.Exit(0);
                }

                // Continue running the application as administrator
                Application.Run(new Form1());
            }
        }

        /// <summary>
        /// Checks whether the current process is running with administrator privileges.
        /// </summary>
        /// <returns>True if running as administrator; otherwise, false.</returns>
        private static bool IsAdministrator()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}
