using Humanizer;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameInput;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.IO;
using Terraria.UI;

namespace DayCounterMod
{
	public class DayCounterMod : Mod
    {
        internal DayUIText text;
        public UserInterface textInterface;

        internal static DayCounterMod instance;

        internal static DayCounterModSystem system;

        public override void Load()
        {
            instance = this;

            if (!Main.dedServ)
            {
                text = new DayUIText();
                text.Initialize();
                textInterface = new UserInterface();
                textInterface.SetState(text);
            }
        }

        public void UpdateUI(GameTime gameTime)
        {
            // it will only draw if the player is not on the main menu
            if (!Main.gameMenu)
                textInterface?.Update(gameTime);
        }

        public void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int inventoryLayerIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text")) - 8;
            layers.Insert(inventoryLayerIndex, new LegacyGameInterfaceLayer("Day Counter: UI", DrawDayCounterUI, InterfaceScaleType.UI));
        }

        private bool DrawDayCounterUI()
        {
            // it will only draw if the player is not on the main menu
            if (!Main.gameMenu)
            {
                if (Main.LocalPlayer?.GetModPlayer<DayCounterPlayer>().showUI ?? false) // check all three booleans that handle drawing the status bars, default to false if it can't access the showUI boolean
                    textInterface.Draw(Main.spriteBatch, new GameTime());
            }
            return true;
        }

        public class DayCounterModSystem : ModSystem
        {
            public ModKeybind ShowUI { get; private set; }
            public ModKeybind SwitchUIState { get; private set; } // switches between total days in world, and total amount of days the player has been thru (player days is saved to the player's .tplr file, which means that it persists across all worlds)

            public int days = 0;
            public int nights = 0;
            public bool tru = true;
            public bool canUpdate = false;

            public override void UpdateUI(GameTime gameTime) => instance.UpdateUI(gameTime);

            public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) => instance.ModifyInterfaceLayers(layers);

            public override void Load()
            {
                base.Load();
                system = this; // this is the modsystem
                ShowUI = KeybindLoader.RegisterKeybind(Mod, "ShowUI", "Period");
                SwitchUIState = KeybindLoader.RegisterKeybind(Mod, "SwitchUIState", "X");
            }

            public override void Unload()
            {
                base.Unload();
                ShowUI = null;
                SwitchUIState = null;
            }

            public override void OnWorldLoad()
            {
                base.OnWorldLoad();
                tru = Main.dayTime;
                canUpdate = true;
            }

            public override void OnWorldUnload()
            {
                days = 0;
                nights = 0;

                canUpdate = false;
                base.OnWorldUnload();
                tru = true;
            }

            public override void PostUpdateTime()
            {
                base.PostUpdateTime();
                if (tru && !Main.dayTime && canUpdate)
                {
                    tru = false;
                    nights++;
                }
                else if (!tru && Main.dayTime && canUpdate)
                {
                    tru = true;
                    days++;
                }
            }

            #region[Saving]
            public override void SaveWorldData(TagCompound tag)
            {
                base.SaveWorldData(tag);

                #region[Saving Values]
                tag.Add("days", days);
                tag.Add("nights", nights);
                tag.Add("tru", tru);
                #endregion
            }

            public override void LoadWorldData(TagCompound tag)
            {
                canUpdate = false;
                days = 0;
                nights = 0;

                base.LoadWorldData(tag);

                #region[Setting Values]
                if (tag.TryGet("days", out int sDays))
                    days = sDays;
                if (tag.TryGet("nights", out int sNights))
                    nights = sNights;
                if (tag.TryGet("tru", out bool sTru))
                    tru = sTru;
                canUpdate = true;
                #endregion
            }
            #endregion
        }
    }

    public class Client : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ClientSide; // only affects how the day counter appears on the client

        [Header("Visual")]
        
        [Label("Show Icon")]
        [DefaultValue(true)]
        [ReloadRequired]
        public bool ShouldShowIcon = true;

        [Label("Day Counter Visible By Default")]
        [DefaultValue(true)]
        [ReloadRequired]
        public bool DayCounterVisibleByDefault = true;

        [Header("Advanced")]

        [Label("World-oriented Text")]
        [DefaultValue("Days: {WORLD_DAYS}")]
        public string ServerDays = "Days: {WORLD_DAYS}";

        [Label("Player-oriented Text")]
        [DefaultValue("Days: {PLAYER_DAYS}")]
        public string PlayerDays = "Days: {PLAYER_DAYS}";

        [Label("Default Type")]
        [DefaultValue(1)]
        [ReloadRequired]
        [Range(1, 2)]
        public int DefaultType = 1;
    }

    public class DayCounterPlayer : ModPlayer // for days that this player has been on a specific world
    {
        public int days = 0;
        public int nights = 0;
        public bool tru = true;
        public bool canUpdate = false;
        public bool showUI = true;
        public bool type = false; // false is represented by the binary bit 0, which in this case is most accurate (and efficient)
        public int temp = -1;
        public bool? tempTru = null;

        public override void OnEnterWorld()
        {
            base.OnEnterWorld();

            tru = Main.dayTime; // set current time
            canUpdate = true;
        }

        public override bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource)
        {
            temp = days;
            tempTru = tru;
            return base.PreKill(damage, hitDirection, pvp, ref playSound, ref genGore, ref damageSource);
        }

        public override void OnRespawn()
        {
            base.OnRespawn();

            if (tempTru == tru && temp != -1 && temp != days) // if this player is the one who respawns
            {
                days = temp;
            }
        }

        public override void Load()
        {
            base.Load();
            showUI = ModContent.GetInstance<Client>().DayCounterVisibleByDefault;
            type = ModContent.GetInstance<Client>().DefaultType != 1;
        }

        public override void Unload()
        {
            days = 0;
            nights = 0;

            canUpdate = false;
            base.Unload();
            tru = true;
        }

        public override void PostUpdate()
        {
            base.PostUpdate();
            if (tru && !Main.dayTime && canUpdate)
            {
                tru = false;
                nights++;
            }
            else if (!tru && Main.dayTime && canUpdate)
            {
                tru = true;
                days++;
            }
        }

        public override void ProcessTriggers(TriggersSet triggersSet) // keybinds
        {
            base.ProcessTriggers(triggersSet);
            DayCounterMod myMod = Mod as DayCounterMod;

            if (myMod != null && DayCounterMod.system != null)
            {
                if (DayCounterMod.system?.ShowUI?.JustPressed == true)
                {
                    showUI = !showUI; // toggle UI
                }
                if (DayCounterMod.system?.SwitchUIState?.JustPressed == true)
                {
                    type = !type; // switch type
                }
            }
        }

        #region[Saving]
        public override void SaveData(TagCompound tag)
        {
            base.SaveData(tag);

            #region[Saving Values]
            tag.Add("days", days);
            tag.Add("nights", nights);
            tag.Add("tru", tru);
            #endregion
        }

        public override void LoadData(TagCompound tag)
        {
            canUpdate = false;
            days = 0;
            nights = 0;

            base.LoadData(tag);

            #region[Setting Values]
            if (tag.TryGet("days", out int sDays))
                days = sDays;
            if (tag.TryGet("nights", out int sNights))
                nights = sNights;
            tru = Main.dayTime;
            canUpdate = true;
            #endregion
        }
        #endregion
    }
}