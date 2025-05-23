﻿using System.Net;
using System.Net.Sockets;

using VoidX.WPF;

using PoorMansAI.Configuration;

namespace PoorMansAI.Extensions {
    /// <summary>
    /// Helps to store the local IP of the Processor on the server. The use case was created by an unconfigurable ISP router, where
    /// the lack of DHCP access made me want to be able to easily check the local IP of the machine that runs the Processor from the Website.
    /// </summary>
    public class LocalIPLogger : WebsiteExtension {
        /// <inheritdoc/>
        protected internal override void Register() {
            PeriodicActions += PeriodicAction;
        }

        /// <summary>
        /// Update the logged local IP address on the Website.
        /// </summary>
        void PeriodicAction() {
            IPAddress[] ips;
            try {
                ips = Dns.GetHostEntry(Dns.GetHostName()).AddressList;
            } catch {
                Logger.Warning("Failed to get local IP addresses. This might be temporary until a new network is connected.");
                return;
            }
            IPAddress localIP = ips.FirstOrDefault(x => x.AddressFamily == AddressFamily.InterNetwork && x.ToString() != "127.0.0.1");
            if (localIP == null) {
                Logger.Warning("No local IPv4 address was found. This might be temporary until a new network is connected.");
                return;
            }

            Logger.Debug("Updating local IP to {0}...", localIP);
            string result = HTTP.POST(HTTP.Combine(Config.publicWebserver, "/cmd/setvar.php"), [
                new KeyValuePair<string, string>("key", "local-ip"),
                new KeyValuePair<string, string>("value", localIP.ToString())
            ], cookies);
            if (result != string.Empty) {
                result ??= "connection error or timeout";
                Logger.Warning("Failed to update local IP: " + result);
            } else {
                Logger.Debug("Updated local IP.");
            }
        }
    }
}