using Newtonsoft.Json;

using Oxide.Core;
using Oxide.Core.Plugins;

using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace Oxide.Plugins
{
    [Info("SimplePatrolSignal", "Yac Vaguer", "1.0.4")]
    [Description("Call a Patrol Helicopter to your location using a special supply signal.")]
    public class SimplePatrolSignal : RustPlugin
    {
        private const string HELI_PREFAB = "assets/prefabs/npc/patrol helicopter/patrolhelicopter.prefab";
        private const string PermissionUse = "simplepatrolsignal.use";

        private PatrolHelicopter patrol;
        private ConfigurationManager config;
        private bool isActive = false;
        private bool originalUseDangerZones;
        private bool originalMonumentCrash;
        private Vector3 patrolZone;
        private Timer patrolTimer;
        private Timer reconsiderTimer;
        private List<LootContainer> processedContainers = new List<LootContainer>();

        #region Initialization

        private void Init()
        {
            permission.RegisterPermission(PermissionUse, this);
            LoadDefaultMessages();
        }

        private void OnServerInitialized()
        {
            LoadConfigValues();
        }

        private void Unload()
        {
            DestroyPatrol();
        }

        #endregion

        #region Localization

        private void LoadDefaultMessages()
        {
            lang.RegisterMessages(new Dictionary<string, string> {
                ["NotAllowed"] = "You are not allowed to use this command.",
                ["HeliSignalActive"] = "Another Patrol Heli Signal is already active.",
                ["PatrolCalled"] = "A patrol helicopter is on its way to your location!",
                ["DestroyingPatrol"] = "The patrol helicopter is leaving the area.",
                ["ReceivedHeliSignal"] = "You have received a Patrol Heli Signal."
            }, this);
        }

        private string GetMessage(string key, string userId = null) => lang.GetMessage(key, this, userId);

        #endregion

        #region Commands

        [ChatCommand("helisignal")]
        private void CmdHeliSignalChat(BasePlayer player, string command, string[] args)
        {
            GiveHeliSignal(player);
        }

        [ConsoleCommand("helisignal")]
        private void CmdHeliSignalConsole(ConsoleSystem.Arg arg)
        {
            var player = arg.Player();
            if (player != null) {
                GiveHeliSignal(player);
            }
        }

        private void GiveHeliSignal(BasePlayer player)
        {
            if (!HasPermission(player)) {
                player.ChatMessage(GetMessage("NotAllowed", player.UserIDString));
                return;
            }

            var supplySignal = CreateHeliSignalItem();
            if (supplySignal != null) {
                player.GiveItem(supplySignal);
                player.ChatMessage(GetMessage("ReceivedHeliSignal", player.UserIDString));
            }
        }

        private bool HasPermission(BasePlayer player)
        {
            return permission.UserHasPermission(player.UserIDString, PermissionUse) || player.IsAdmin;
        }

        #endregion

        #region Event Hooks

        private void OnExplosiveThrown(BasePlayer player, BaseEntity entity)
        {
            if (entity is SupplySignal signal && signal.skinID == config.Signal.SkinId) {
                signal.CancelInvoke(signal.Explode);
                signal.Invoke(signal.KillMessage, 30f);

                timer.Once(config.Signal.Warmup, () => {
                    CallPatrolHelicopter(player);
                    signal.Kill();
                });
            }
        }

        private object CanLootEntity(BasePlayer player, LootContainer container)
        {
            if (container == null || !config.LootSettings.Enabled)
                return null;

            if (processedContainers.Contains(container))
                return null;

            processedContainers.Add(container);

            string containerName = container.ShortPrefabName;
            float dropChance;

            if (config.LootSettings.Containers.TryGetValue(containerName, out dropChance)) {
                if (RollChance(dropChance)) {
                    var item = CreateHeliSignalItem();
                    if (item != null) {
                        container.inventory.capacity++;
                        container.inventorySlots++;
                        item.MoveToContainer(container.inventory);
                    }
                }
            }

            return null;
        }

        private void OnEntityKill(LootContainer container)
        {
            if (container != null) {
                processedContainers.Remove(container);
            }
        }

        #endregion

        #region Patrol Helicopter Logic

        private void CallPatrolHelicopter(BasePlayer player)
        {
            if (isActive) {
                player.ChatMessage(GetMessage("HeliSignalActive", player.UserIDString));
                GiveHeliSignal(player);
                return;
            }

            isActive = true;
            patrolZone = player.transform.position;
            SpawnPatrolHelicopter();
            player.ChatMessage(GetMessage("PatrolCalled", player.UserIDString));
        }

        private void SpawnPatrolHelicopter()
        {
            originalUseDangerZones = PatrolHelicopterAI.use_danger_zones;
            originalMonumentCrash = PatrolHelicopterAI.monument_crash;

            Vector3 spawnPosition = patrolZone + (Vector3.forward * 500f);
            spawnPosition.y = 60f;

            patrol = GameManager.server.CreateEntity(HELI_PREFAB, spawnPosition) as PatrolHelicopter;

            if (patrol == null) {
                PrintError("Failed to create Patrol Helicopter entity.");
                isActive = false;
                return;
            }

            patrol.enableSaving = false;
            patrol.Spawn();

            patrol.InitializeHealth(config.Patrol.Health, config.Patrol.Health);
            if (patrol.weakspots != null) {
                if (patrol.weakspots.Length > 0) {
                    patrol.weakspots[0].maxHealth = config.Patrol.MainRotorHealth;
                    patrol.weakspots[0].health = config.Patrol.MainRotorHealth;
                }
                if (patrol.weakspots.Length > 1) {
                    patrol.weakspots[1].maxHealth = config.Patrol.TailRotorHealth;
                    patrol.weakspots[1].health = config.Patrol.TailRotorHealth;
                }
            }

            patrol.myAI.timeBetweenRockets = config.Patrol.TimeBeforeRocket;
            patrol.maxCratesToSpawn = config.Patrol.CrateAmount;
            PatrolHelicopterAI.use_danger_zones = false;
            PatrolHelicopterAI.monument_crash = false;
            patrol.myAI.hasInterestZone = true;
            patrol.myAI.interestZoneOrigin = patrolZone;
            patrol.myAI.ExitCurrentState();

            reconsiderTimer = timer.Repeat(10f, 0, ReconsiderPosition);
            patrolTimer = timer.Once(config.Patrol.Duration, DestroyPatrol);
        }

        private void ReconsiderPosition()
        {
            if (patrol == null || patrol.IsDestroyed) {
                DestroyPatrol();
                return;
            }

            if (patrol.myAI.leftGun.HasTarget() || patrol.myAI.rightGun.HasTarget()) {
                return;
            }

            patrol.myAI.State_Move_Enter(patrolZone);
        }

        private void DestroyPatrol()
        {
            Puts(GetMessage("DestroyingPatrol"));

            if (reconsiderTimer != null && !reconsiderTimer.Destroyed) {
                reconsiderTimer.Destroy();
            }

            if (patrol != null && !patrol.IsDestroyed) {
                patrol.myAI.Retire();
                patrol.Kill();
                patrol = null;
            }

            PatrolHelicopterAI.use_danger_zones = originalUseDangerZones;
            PatrolHelicopterAI.monument_crash = originalMonumentCrash;
            isActive = false;
        }

        #endregion

        #region Helper Methods

        private Item CreateHeliSignalItem()
        {
            var item = ItemManager.CreateByName("supply.signal", 1, config.Signal.SkinId);
            if (item != null) {
                item.name = config.Signal.DisplayName;
            }
            return item;
        }

        private bool RollChance(float chance)
        {
            return UnityEngine.Random.Range(0f, 100f) <= chance;
        }

        #endregion

        #region Configuration

        private class ConfigurationManager
        {
            [JsonProperty("Supply Signal Settings")]
            public SupplySignalSettings Signal { get; set; } = new SupplySignalSettings();

            [JsonProperty("Patrol Helicopter Settings")]
            public PatrolSettings Patrol { get; set; } = new PatrolSettings();

            [JsonProperty("Loot Settings")]
            public LootSettings LootSettings { get; set; } = new LootSettings();
        }

        private class SupplySignalSettings
        {
            [JsonProperty("Skin ID")]
            public ulong SkinId { get; set; } = 3050057945;

            [JsonProperty("Display Name")]
            public string DisplayName { get; set; } = "Patrol Heli Signal";

            [JsonProperty("Warmup Time Before Patrol Arrival (seconds)")]
            public float Warmup { get; set; } = 5f;
        }

        private class PatrolSettings
        {
            [JsonProperty("Patrol Duration (seconds)")]
            public float Duration { get; set; } = 300f;

            [JsonProperty("Helicopter Health")]
            public float Health { get; set; } = 10000f;

            [JsonProperty("Main Rotor Health")]
            public float MainRotorHealth { get; set; } = 900f;

            [JsonProperty("Tail Rotor Health")]
            public float TailRotorHealth { get; set; } = 500f;

            [JsonProperty("Number of Crates to Spawn")]
            public int CrateAmount { get; set; } = 6;

            [JsonProperty("Time Before Firing Rockets (seconds)")]
            public float TimeBeforeRocket { get; set; } = 0.25f;
        }

        private class LootSettings
        {
            [JsonProperty("Enable Loot Drops")]
            public bool Enabled { get; set; } = true;

            [JsonProperty("Loot Containers and Drop Chances")]
            public Dictionary<string, float> Containers { get; set; } = new Dictionary<string, float>
            {
                { "crate_normal", 5f },
                { "crate_normal_2", 5f },
                { "crate_elite", 10f },
                { "heli_crate", 15f },
                { "bradley_crate", 15f }
            };
        }

        protected override void LoadDefaultConfig()
        {
            config = new ConfigurationManager();
            SaveConfig();
            Puts("Default configuration file created.");
        }

        private void LoadConfigValues()
        {
            try {
                config = Config.ReadObject<ConfigurationManager>();
                if (config == null) {
                    LoadDefaultConfig();
                } else {
                    SaveConfig();
                }
            } catch {
                PrintError("Configuration file is corrupt; loading default configuration.");
                LoadDefaultConfig();
            }
        }

        private void SaveConfig()
        {
            Config.WriteObject(config, true);
        }

        #endregion
    }
}
