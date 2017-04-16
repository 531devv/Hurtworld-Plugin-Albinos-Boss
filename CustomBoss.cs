using UnityEngine;
using Oxide.Core;
using System;
using System.Collections.Generic;
using Oxide.Core.Libraries.Covalence;
using System.Collections.ObjectModel;
namespace Oxide.Plugins
{
    [Info("CustomBoss", "531devv", "1.0.1", ResourceId = 531)]
    [Description("Spawn the creatures!")]

    class CustomBoss : HurtworldPlugin
    {
        DefaultConfig config;
        public static GameObject gameObject;
        static readonly DateTime epoch = new DateTime(2017, 1, 13, 17, 44, 0);
        static double CurrentTime() { return DateTime.UtcNow.Subtract(epoch).TotalSeconds; }

        #region Configuration

        #region Init
        void Init()
        {
            try
            {
                config = Config.ReadObject<DefaultConfig>();
            }
            catch

            {
                PrintWarning("Could not read config, creating new default config");
                LoadDefaultConfig();
            }

            try
            {
                StartTimer(); Puts("Start a timer!");
            }
            catch
            {
                PrintWarning("Timer doesnt work!");
            }
        }
        #endregion

        class DefaultConfig
        {
        }

        protected override void LoadDefaultConfig()
        {
            PrintWarning("Creating a new configuration file");
            Config.Clear();
            config = new DefaultConfig();
            Config.WriteObject(config, true);
            SaveConfig();
        }

        #endregion

        #region DataManager
        List<BossData> Datas = new List<BossData>();
        string editAreaName = null;
        BossData data;

        class BossData
        {
            private HashSet<string> received = new HashSet<string>();

            public string name { get; set; }
            public double timer { get; set; }

            public BossData() { }

            public void ClearReceived()
            {
                this.received.Clear();
            }

            public BossData(string name, double timer)
            {
                this.name = name;
                this.timer = timer;
            }

            public void RemoveReceived(string name)
            {
                this.received.Remove(name);
            }
        }

        private void LoadBossData()
        {
            var _Data = Interface.GetMod().DataFileSystem.ReadObject<Collection<BossData>>("BossData");
            foreach (var item in _Data)
            {
                Datas.Add(new BossData(
                        item.name,
                        item.timer
                    ));
            }
        }

        private void SaveBossData()
        {
            Interface.GetMod().DataFileSystem.WriteObject("BossData", Datas);
        }

        void Loaded()
        {
            SetTimer();
            LoadBossData();
        }

        void OnPlayerConnected(PlayerSession p)
        {
            hurt.SendChatMessage(p, "<color=#008080ff>Na serwerze jest wtyczka gniazdo albinosów</color>");
            hurt.SendChatMessage(p, "Gniazdo znajduje się w okolicach bambika!");
            hurt.SendChatMessage(p, "<i>/boss czas</i>");
        }

        #endregion

        #region Timers

        void SetTimer()
        {
            double date = CurrentTime() + 10800;
            Datas.Add(new BossData("Boss", date));
            SaveBossData();
        }

        void StartTimer()
        {
            timer.Repeat(1f, 0, () =>
            {
                CheckTime();
            });

            timer.Repeat(10800f, 0, () =>
            {
                timer.Once(7200, () =>
                {
                    Puts("1 hour remaining to spawn creatures");
                    hurt.BroadcastChat("<color=#008080ff>Została godzina do wyklucia się </color>albinosów!");
                });

                timer.Once(3600, () =>
                {
                    Puts("2 hour remaining to spawn creatures");
                    hurt.BroadcastChat("<color=#008080ff>Dwie godziny do wyklucia się </color>albinosów!");
                });

                SpawnObject("AITokarAlbinoServer", -3322, 249, -2619);
                SpawnObject("AITokarAlbinoServer", -3322, 249, -2619);
                SpawnObject("AITokarAlbinoServer", -3322, 249, -2619);
                SpawnObject("AITokarAlbinoServer", -3322, 249, -2619);
                SpawnObject("AITokarAlbinoServer", -3322, 249, -2619);
                SpawnObject("AITokarAlbinoServer", -3322, 249, -2619);
                SpawnObject("AITokarAlbinoServer", -3322, 249, -2619);
                SpawnObject("AITokarAlbinoServer", -3322, 249, -2619);
                Puts("Creatures was spawned");
                hurt.BroadcastChat("Albinosy <color=#008080ff>wykluwają się z jajek!</color>");
                hurt.BroadcastChat("Masz godzinę,<color=#008080ff> zabij je i zdobądź łup!</color>");
            });
        }

        void CheckTime()
        {
            var _Data = Interface.GetMod().DataFileSystem.ReadObject<Collection<BossData>>("BossData");
            foreach (var item in _Data)
            {
                Datas.Add(new BossData(
                        item.name,
                        item.timer
                    ));
                double ct = CurrentTime();
                double cd = item.timer;
                double finish = cd - ct;
                int toInt = (int)finish;
                if (toInt == 0)
                {
                    Datas.Clear();
                    item.RemoveReceived("Boss");
                    item.ClearReceived();
                    SetTimer();
                    LoadDefaultConfig();
                }
            }

        }

        void GetTime(PlayerSession p)
        {

            var _Data = Interface.GetMod().DataFileSystem.ReadObject<Collection<BossData>>("BossData");
            foreach (var item in _Data)
            {
                Datas.Add(new BossData(
                        item.name,
                        item.timer
                    ));
                double ct = CurrentTime();
                double cd = item.timer;
                double finish = cd - ct;
                double remained = finish / 60;
                int toInt = (int)remained;
                hurt.SendChatMessage(p, toInt.ToString() + " minut.");
            }
        }
        #endregion

        #region Body
        public void SpawnObject(string monster, float x, float y, float z)
        {
            Vector3 position = new Vector3(x + Core.Random.Range(0, 2),
                    y + Core.Random.Range(0, 2),
                    z + Core.Random.Range(0, 1));
            RaycastHit hitInfo;
            Physics.Raycast(position, Vector3.down, out hitInfo);
            {
                Quaternion rotation = Quaternion.Euler(0.0f, (float)UnityEngine.Random.Range(0f, 360f), 0.0f);
                rotation = Quaternion.FromToRotation(Vector3.down, hitInfo.normal) * rotation;
                gameObject = Singleton<HNetworkManager>.Instance.NetInstantiate(monster, hitInfo.point, Quaternion.identity, GameManager.GetSceneTime());
                Destroy(gameObject);
            }
        }

        void Destroy(GameObject obj)
        {
            timer.Once(3600, () =>
            {
                Singleton<HNetworkManager>.Instance.NetDestroy(uLink.NetworkView.Get(obj));
                Puts("Tokars was destroyed!");
            });
        }

        [ChatCommand("boss")]
        void cmdBoss(PlayerSession p, string command, string[] args)
        {
            if (args.Length < 1)
            {
                hurt.SendChatMessage(p, "===============");
                hurt.SendChatMessage(p, "Komendy gracza:");
                hurt.SendChatMessage(p, "<i>/boss czas</i>");
                hurt.SendChatMessage(p, "<i>/boss autor</i>");
                if (p.IsAdmin == true)
                {
                    hurt.SendChatMessage(p, "Komendy admina:");
                    hurt.SendChatMessage(p, "<i>/boss event</i>");
                    hurt.SendChatMessage(p, "===============");
                    return;
                }
                hurt.SendChatMessage(p, "===============");
                return;
            }

            if (args[0].Equals("autor"))
            {
                hurt.SendChatMessage(p, "===============");
                hurt.SendChatMessage(p, "<color=#008080ff>telegram:</color> devv531");
                hurt.SendChatMessage(p, "<color=#008080ff>ts3:</color> kingspeak.net");
                hurt.SendChatMessage(p, "<color=#008080ff>@:</color> 531devv@gmail.com");
                hurt.SendChatMessage(p, "===============");
                return;
            }
            if (args[0].Equals("czas"))
            {
                hurt.SendChatMessage(p, "===============");
                hurt.SendChatMessage(p, "<i>Albinosy</i> <color=#008080ff>wyklują się za:</color>");
                GetTime(p);
                hurt.SendChatMessage(p, "===============");
                return;
            }

            if (p.IsAdmin == true)
            {
                if (args[0].Equals("event"))
                {
                    SpawnObject("AITokarAlbinoServer", -3322, 249, -2619);
                }
                return;
            }
            return;
        }

        #endregion

        T GetConfig<T>(string name, T value) => Config[name] == null ? value : (T)Convert.ChangeType(Config[name], typeof(T));
    }
}

