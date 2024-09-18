using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.UI;

namespace DayCounterMod
{
    public class DayUIText : UIState
    {
        public UIElement area;
        public UIImage icon;
        public UIText text;

        public override void OnInitialize()
        {
            area = new UIElement();
            area.Width.Set(182, 0);
            area.Height.Set(60, 0);
            area.HAlign = 0;
            area.VAlign = 1;
            area.Top.Set(-7, 0);
            area.Left.Set(10, 0);

            text = new UIText(ModContent.GetInstance<Client>().ServerDays.Replace("{WORLD_DAYS}", "0").Replace("{WORLD_NIGHTS}", "0").Replace("{PLAYER_DAYS}", "0").Replace("{PLAYER_NIGHTS}", "0"), (float) 0.8);
            text.HAlign = 0;
            text.Width.Set(138, 0);
            text.Height.Set(34, 0);
            text.Top.Set(40, 0);
            text.Left.Set(48, 0);
            text.TextOriginX = 0;

            if (ModContent.GetInstance<Client>().ShouldShowIcon)
            {
                icon = new UIImage(ModContent.Request<Texture2D>("DayCounterMod/sun"));
                icon.Width.Set(32, 0);
                icon.Height.Set(32, 0);
                icon.Left.Set(12, 0);
                icon.VAlign = 1;
                area.Append(icon);
            }

            area.Append(text);
            Append(area);
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            base.DrawSelf(spriteBatch);
            string currentText = Main.LocalPlayer.GetModPlayer<DayCounterPlayer>().type == false ? ModContent.GetInstance<Client>().ServerDays.Replace("{WORLD_DAYS}", DayCounterMod.system.days.ToString()).Replace("{WORLD_NIGHTS}", DayCounterMod.system.nights.ToString()).Replace("{PLAYER_DAYS}", Main.LocalPlayer.GetModPlayer<DayCounterPlayer>().days.ToString()).Replace("{PLAYER_NIGHTS}", Main.LocalPlayer.GetModPlayer<DayCounterPlayer>().nights.ToString()) : ModContent.GetInstance<Client>().PlayerDays.Replace("{WORLD_DAYS}", DayCounterMod.system.days.ToString()).Replace("{WORLD_NIGHTS}", DayCounterMod.system.nights.ToString()).Replace("{PLAYER_DAYS}", Main.LocalPlayer.GetModPlayer<DayCounterPlayer>().days.ToString()).Replace("{PLAYER_NIGHTS}", Main.LocalPlayer.GetModPlayer<DayCounterPlayer>().nights.ToString());
            text.SetText(currentText);
        }
    }
}
