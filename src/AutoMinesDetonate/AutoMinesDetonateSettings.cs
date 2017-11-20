using PoeHUD.Hud.Settings;
using PoeHUD.Plugins;

namespace AutoMinesDetonate
{
    public class AutoMinesDetonateSettings : SettingsBase
    {
        public AutoMinesDetonateSettings()
        {
            Enable = true;
            //UseTremor = false;
            UseTotem = true;
            NeedMines = new RangeNode<int>(3, 1, 16);
            Minions = new RangeNode<int>(1, 0, 8);
            Delay = new RangeNode<int>(750, 500, 2000);
        }
        //[Menu("Use Tremor?")]
        //public ToggleNode UseTremor { get; set; }

        [Menu("Disable when use totem?")]
        public ToggleNode UseTotem { get; set; }

        [Menu("Need Mines:")]
        public RangeNode<int> NeedMines { get; set; }

        [Menu("Delay:")]
        public RangeNode<int> Delay { get; set; }

        [Menu("Have minions:")]
        public RangeNode<int> Minions { get; set; }
    }
}
