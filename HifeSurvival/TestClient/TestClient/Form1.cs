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
        /*
         *  1 연결 상태 , 0: 연결 끊김, 1 : 연결 됨
         *  2 게임 진입,  0: 진입, 1: 준비, 2: 카운트 다운, 3: 로드 게임, 4: 게임 시작, 5: 게임 종료
         * 
         */

        public Form1()
        {
            InitializeComponent();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (_sesh != null)
            {
                var playerEntity = _sesh.Player;
                if (playerEntity == null)
                {
                    if (_sesh.IsConntected)
                    {
                        _status = (1, 1);
                        button1.Enabled = true;
                    }
                    else
                    {
                        _status = (1, 0);
                        button1.Enabled = false;
                    }
                }
                else
                {
                    if(playerEntity.GameModeStatus == 0)
                    {
                        _status = (2, 0);
                    }
                    
                    label3.Text = playerEntity.Id.ToString();
                }
            }
            else
            {
                label2.Text = "Disconnected -";
                button1.Enabled = false;
            }



            if (_lastStatus != _status)
            {
                switch(_status.mainStatus)
                {
                    case 1:
                        {
                            label2.Text = _status.subStatus == 1 ? "Connected" : "Disconnected";
                        }
                        break;
                    case 2:
                        {
                            label2.Text = _status.subStatus switch
                            {
                                0 => "RoomEntered",
                                1 => "Ready",
                                2 => "CountDown",
                                3 => "LoadGame",
                                4 => "PlayStart",
                                5 => "FinishGame",
                                _ => "Invalid",
                            };
                        }
                        break;
                    default:
                        break;
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
            var endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 7777);

            PacketManager.Instance.BindHandler(new ClientPacketHandler());

            Connector connector = new Connector();

            _sesh = SessionManager.Instance.Generate();
            connector.Connect(endPoint,
                () => { return _sesh; },
                1);
        }

    }
}
