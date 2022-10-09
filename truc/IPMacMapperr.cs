using System;
using System.Diagnostics;

namespace truc
{
    public class IPMacMapper
    {
        private static List<IPAndMac> list;

        private static StreamReader ExecuteCommandLine(String file, String arguments = "")
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = true;
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;
            startInfo.FileName = file;
            startInfo.Arguments = arguments;

            Process process = Process.Start(startInfo);

            return process.StandardOutput;
        }

        private static void InitializeGetIPsAndMac()
        {
            if (list != null)
                return;

            var arpStream = ExecuteCommandLine("arp", "-a");
            List<string> result = new List<string>();
            while (!arpStream.EndOfStream)
            {
                var line = arpStream.ReadLine().Trim();
                result.Add(line);
            }

            list = result.Where(x => !string.IsNullOrEmpty(x))
                .Select(x =>
                {
                    string[] parts = x.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                    return new IPAndMac { IP = parts[1].Trim(), MAC = parts[3].Trim() };
                }).ToList();
        }

        public static string FindIPFromMacAddress(string macAddress)
        {
            InitializeGetIPsAndMac();
            IPAndMac item = list.SingleOrDefault(x => x.MAC == macAddress);
            if (item == null)
                return null;
            return item.IP;
        }

        public static string FindMacFromIPAddress(string ip)
        {
            InitializeGetIPsAndMac();
            IPAndMac item = list.SingleOrDefault(x => x.IP == ip);
            if (item == null)
                return null;
            return item.MAC;
        }

        private class IPAndMac
        {
            public string IP { get; set; }
            public string MAC { get; set; }
        }
    }
}

