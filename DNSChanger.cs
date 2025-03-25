using System;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;

namespace AdBlocker
{
    internal static class DNSChanger
    {
        /// <summary>
        /// Checks if any active interface has both preferred and alternate DNS set.
        /// </summary>
        public static bool IsDnsSetTo(string preferredDns, string alternateDns)
        {
            var activeInterfaces = NetworkInterface.GetAllNetworkInterfaces()
                                                   .Where(adapter => adapter.OperationalStatus == OperationalStatus.Up);

            foreach (var adapter in activeInterfaces)
            {
                var ipv4DnsAddresses = adapter.GetIPProperties().DnsAddresses
                                              .Where(dns => dns.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                                              .Select(dns => dns.ToString());

                if (ipv4DnsAddresses.Contains(preferredDns) && ipv4DnsAddresses.Contains(alternateDns))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Sets the DNS servers for all active network interfaces.
        /// </summary>
        public static void SetDnsForAllInterfaces(string primaryDns, string alternateDns, bool isIPv4)
        {
            string version = isIPv4 ? "ip" : "ipv6";
            var interfaceNames = GetActiveInterfaceNames();

            foreach (var interfaceName in interfaceNames)
            {
                string primaryCommand = $"netsh interface {version} set dns name=\"{interfaceName}\" static {primaryDns}";
                RunCommand(primaryCommand);

                if (!string.IsNullOrWhiteSpace(alternateDns))
                {
                    string alternateCommand = $"netsh interface {version} add dns name=\"{interfaceName}\" {alternateDns} index=2";
                    RunCommand(alternateCommand);
                }
            }
        }

        /// <summary>
        /// Restores DNS settings to default (DHCP) for all active network interfaces.
        /// </summary>
        public static void RestoreDefaultDnsForAllInterfaces()
        {
            var interfaceNames = GetActiveInterfaceNames();

            foreach (var interfaceName in interfaceNames)
            {
                string resetIPv4Command = $"netsh interface ip set dns name=\"{interfaceName}\" source=dhcp";
                string resetIPv6Command = $"netsh interface ipv6 set dns name=\"{interfaceName}\" source=dhcp";

                RunCommand(resetIPv4Command);
                RunCommand(resetIPv6Command);
            }
            Console.WriteLine("DNS settings restored to default for all interfaces.");
        }

        /// <summary>
        /// Retrieves the names of all active network interfaces.
        /// </summary>
        private static string[] GetActiveInterfaceNames()
        {
            return NetworkInterface.GetAllNetworkInterfaces()
                                   .Where(adapter => adapter.OperationalStatus == OperationalStatus.Up)
                                   .Select(adapter => adapter.Name)
                                   .ToArray();
        }

        /// <summary>
        /// Runs a command with administrator privileges and returns its output.
        /// </summary>
        private static string RunCommand(string command)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/C {command}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                };

                using (Process process = Process.Start(psi))
                {
                    if (process == null)
                    {
                        throw new InvalidOperationException("Failed to start process.");
                    }

                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();
                    process.WaitForExit();

                    if (!string.IsNullOrWhiteSpace(error))
                    {
                        Console.Error.WriteLine("Error: " + error);
                    }
                    return output;
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Exception while executing command '{command}': {ex.Message}");
                return string.Empty;
            }
        }
    }
}
