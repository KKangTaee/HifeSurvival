using System;
using System.Linq;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using ServerCore;

namespace TestClient
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList.First(ip => ip.AddressFamily == AddressFamily.InterNetwork);
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

            endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 7777);
            // System.Console.WriteLine(ipAddr.Address);

            PacketManager.Instance.BindHandler(new ClientPacketHandler());

            Connector connector = new Connector();

            connector.Connect(endPoint,
                () => { return SessionManager.Instance.Generate(); },
                1);
        }
    }
}
