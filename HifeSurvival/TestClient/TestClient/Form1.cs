using System;
using System.Linq;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using ServerCore;

namespace TestClient
{
    public partial class Form1 : Form
    {
        public static GameRoom Room;
        public static ConcurrentQueue<string> LogMsgQ = new ConcurrentQueue<string>();
        private static Timer _updateTimer = new Timer();
        private ClientSession _sesh;

        private (int mainStatus, int subStatus) _lastStatus = default;
        private (int mainStatus, int subStatus) _status = default;
        /*
         *  0 연결 상태 , 0: 연결 끊김, 1 : 연결 됨
         *  1 게임 진입,  0: 진입, 1: 준비, 2: 카운트 다운, 3: 로드 게임, 4: 게임 시작, 5: 게임 종료
         * 
         */

        public Form1()
        {
            InitializeComponent();
            _updateTimer.Tick += new EventHandler(Update);
            _updateTimer.Interval = 100;
            _updateTimer.Start();
        }

        private void Update(object obj, EventArgs args)
        {
            CurrencyTextBox.Text = dropItemListBox.Text = ItemListTextBox.Text =  string.Empty;

            if (_sesh != null)
            {
                if (_sesh.IsConntected)
                {
                    _status = (0, 1);
                }
                else
                {
                    _status = (0, 0);
                    _sesh.Player = null;
                }

                var playerEntity = _sesh.Player;
                if (playerEntity != null)
                {
                    _status = (1, playerEntity.GameModeStatus);
                    if (_status.subStatus == 2)
                    {
                        playerEntity.CountDownSec -= 100;

                        label2.Text = $"CountDown {(int)(_sesh.Player.CountDownSec * 0.001f)}";
                    }
                }
            }
            else
            {
                _status = (0, 0);
            }



            if (_lastStatus != _status)
            {
                switch (_status.mainStatus)
                {
                    case 0:
                        {
                            if (Room != null)
                            {
                                Room = null;
                            }

                            if (_status.subStatus == 0)
                            {
                                label2.Text = "Disconnected";
                                startgameBtn.Enabled = false;
                                connectBtn.Enabled = true;
                                testBtn.Enabled = false;
                            }
                            else
                            {
                                label2.Text = "Connected";
                                startgameBtn.Enabled = true;
                                connectBtn.Enabled = false;
                                testBtn.Enabled = true;
                            }
                        }
                        break;
                    case 1:
                        {
                            label3.Text = "ID : " + _sesh.Player.Id.ToString();
                            if (_sesh.Player.HeroKey != 0)
                            {
                                label3.Text += $" / Hero key : {_sesh.Player.HeroKey}";
                            }

                            if(_status.subStatus == 3)
                            {
                                Room = new GameRoom();
                            }

                            startgameBtn.Enabled = false;
                            testBtn.Enabled = true;
                            label2.Text = _status.subStatus switch
                            {
                                0 => "RoomEntered",
                                1 => "Ready",
                                //2 => $"CountDown {(int)(_sesh.Player.CountDownSec* 0.001f)}",
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


            if (_sesh != null)
            {
                if (_sesh.Player != null)
                {
                    var player = _sesh.Player;

                    if (!_sesh.Player.OriginStat.Equals(default(PStat)))
                    {
                        string statFormat =
@"Str : {0} (+{3})
Def : {1} (+{4})
Hp : {2} (+{5})";
                        StatTextBox1.Text = string.Format(statFormat,
                            _sesh.Player.OriginStat.str,
                            _sesh.Player.OriginStat.def,
                            _sesh.Player.OriginStat.hp,
                            _sesh.Player.AdditionalStat.str,
                            _sesh.Player.AdditionalStat.def,
                            _sesh.Player.AdditionalStat.hp);
                    }


                    string currencyFormat = "{0} : {1}\n";
                    foreach (var c in player.CurrencyList)
                    {
                        CurrencyTextBox.Text += string.Format(currencyFormat,
                            c.currencyType switch
                            {
                                1 => "gold",
                                _ => "",
                            }, c.count);
                    }

                    string itemFormat =
@"-slot {0}
key {1}, level {2}, stack {3}/{4}
";

                    foreach (var item in player.InvenItemDict)
                    {
                        ItemListTextBox.Text += string.Format(itemFormat, item.Key, item.Value.itemKey, item.Value.itemLevel, item.Value.currentStack, item.Value.maxStack);
                    }


                    if(Room != null)
                    {
                        string dropFormat =
@" drop ID : {0} - {1}";

                        foreach(var dropItem in Room.DropDict)
                        {
                            dropItemListBox.Text += string.Format(dropFormat, dropItem.Key, dropItem.Value.type == 1 ? "gold" : "item");
                        }
                    }

                }
            }

            while (!LogMsgQ.IsEmpty)
            {
                if (LogMsgQ.TryDequeue(out var logMsg))
                {
                    LogTextBox.Text += $"{logMsg}\r\n";
                }
            }


            JobTimer.Instance.Flush();
        }


        private void StartGameClick(object sender, EventArgs e)
        {
            if (_status.mainStatus == 0 && _status.subStatus == 1)
            {
                var req = new C_JoinToGame()
                {
                    userId = DEFINE.TEST_USER_ID,
                    userName = "testClient",
                };
                _sesh.Send(req.Write());
            }
        }

        private void TestClick(object sender, EventArgs e)
        {
            if (_status.mainStatus == 0 && _status.subStatus == 0)
            {
                return;
            }

            _sesh.Cheat("equipitem 1");
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


        private void CheatBtnClick(object sender, EventArgs e)
        {
            if (_status.mainStatus == 0 && _status.subStatus == 0)
            {
                return;
            }

            _sesh.Cheat(cheatCommandBox.Text);
        }
    }
}
