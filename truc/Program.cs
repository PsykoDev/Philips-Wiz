using System.Net;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;

namespace truc;



class Program
{

    static int PORT = 38899;
    static List<string> ipAddress = new()
    {
        "192.168.1.80",
        "192.168.1.100",
        "192.168.1.31",
        "192.168.1.35",
    };

    static void Main(string[] args)
    { 
        detect();

        Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        for(int i =0; i< ipAddress.Count; i++)
        {

            IPAddress serverAddr = IPAddress.Parse(ipAddress[i]);

            IPEndPoint endPoint = new IPEndPoint(serverAddr, PORT);

            string text = "{\"id\":1,\"method\":\"setState\",\"params\":{\"state\":false}}";
            byte[] send_buffer = Encoding.ASCII.GetBytes(text);

            sock.SendTo(send_buffer, endPoint);
        }
    }

    private static void detect()
    {
        UdpClient udpClient = new UdpClient();
        udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, PORT));

        var from = new IPEndPoint(0, 0);
        int i = 0;
        var task = Task.Run(() =>
        {
            while (i < 255)
            {
                var recvBuffer = udpClient.Receive(ref from);
                dynamic parse = Encoding.UTF8.GetString(recvBuffer);
                Parse myDeserializedClass = JsonConvert.DeserializeObject<Parse>(parse);
                string macupdate = getFormatMac(myDeserializedClass.result?.mac);
                Console.WriteLine($"{macupdate} {IPMacMapper.FindIPFromMacAddress(macupdate)} {myDeserializedClass.result?.state}");
                Console.WriteLine(parse);
                if(IPMacMapper.FindIPFromMacAddress(macupdate) != null)
                    //ipAddress.Add(IPMacMapper.FindIPFromMacAddress(macupdate).Replace("(", "").Replace(")", ""));
                i++;
            }
        });

        var data = Encoding.UTF8.GetBytes("{\"method\":\"getPilot\",\"params\":{}}");
        udpClient.Send(data, data.Length, "255.255.255.255", PORT);

        task.Wait(2000);
    }

    static string getFormatMac(string sMacAddress)
    {
        if (sMacAddress == null) return "";
        string MACwithColons = "";
        for (int i = 0; i < sMacAddress.Length; i++)
        {
            MACwithColons = MACwithColons + sMacAddress.Substring(i, 2) + ":";
            i++;
        }
        MACwithColons = MACwithColons.Substring(0, MACwithColons.Length - 1);

        return MACwithColons;
    }
}

