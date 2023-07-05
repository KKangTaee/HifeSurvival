using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using ServerCore;

namespace TestClient
{
    public partial class Form1 : Form
    {
        private ClientSession _sesh;

        private (int mainStatus, int subStatus) _lastStatus = default;
        private (int mainStatus, int subStatus) _status = default;

        public Form1()
        {
            InitializeComponent();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if(_sesh != null)
            {
                if (_sesh.IsConntected)
                {
                    _status = (1, 1);
                }
                else
                {
                    _status = (1, 0);
                }
            }


            if(_lastStatus != _status)
            {
                if(_status.mainStatus == 1)
                {
                    label2.Text = _status.subStatus == 1 ? "Connected" : "Disconnected";
                }

                _lastStatus = _status;
            }

        }


        private void StartGameClick(object sender, EventArgs e)
        {

        }

        private void TestClick(object sender, EventArgs e)
        {

        }

        private void ConnectBtnClick(object sender, EventArgs e)
        {
            var host = Dns.GetHostName();
            var ipHost = Dns.GetHostEntry(host);
            var endPoint =  new IPEndPoint(IPAddress.Parse("127.0.0.1"), 7777);

            PacketManager.Instance.BindHandler(new ClientPacketHandler());

            Connector connector = new Connector();

            _sesh = SessionManager.Instance.Generate();
            connector.Connect(endPoint,
                () => { return _sesh; },
                1);
        }

    }
}
