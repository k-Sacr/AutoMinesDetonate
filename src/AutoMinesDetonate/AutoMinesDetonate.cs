using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using PoeHUD.Models;
using PoeHUD.Plugins;
using PoeHUD.Poe.Components;

namespace AutoMinesDetonate
{
    public class AutoMinesDetonate : BaseSettingsPlugin<AutoMinesDetonateSettings>
    {
        private List<int> _minions;
        private Thread _thread;
        private readonly Random _random = new Random();
        private bool _run;
        private int _settingMinion;
        private readonly HashSet<EntityWrapper> _activeTotems = new HashSet<EntityWrapper>();

        public override void Initialise()
        {
            _settingMinion = _minions.Count - Settings.Minions.Value;
        }

        public override void EntityAdded(EntityWrapper entity)
        {
            if (entity != null && !entity.IsHostile && entity.Path.Contains("StrengthTotem"))
            {
                _activeTotems.Clear();
                _activeTotems.Add(entity);
            }
        }

        public override void EntityRemoved(EntityWrapper entity)
        {
            //_minions.Remove(entity);
            if (entity != null && !entity.IsHostile && entity.Path.Contains("StrengthTotem"))
                _activeTotems.Remove(entity);
        }

        public override void Render()
        {
            if (Settings.Enable)
            {
                try
                {
                    if (GameController.Game.IngameState.IngameUi.InventoryPanel.IsVisible
                        || GameController.Game.IngameState.IngameUi.OpenLeftPanel.IsVisible
                        || GameController.Game.IngameState.IngameUi.AtlasPanel.IsVisible
                        || GameController.Game.IngameState.IngameUi.OpenRightPanel.IsVisible)
                    {
                        _activeTotems.Clear();
                        return;
                    }
                    if (Settings.UseTotem && _activeTotems.Count > 0)
                    {
                        _run = false;
                        return;
                    }
                    var data = GameController.Game.IngameState.Data;
                    _minions = data.LocalPlayer.GetComponent<Actor>().Minions;
                    //int count = 0;
                    //foreach (var minionId in _minions)
                    //{
                    //    if (data.EntityList.EntitiesAsDictionary.ContainsKey(minionId))
                    //    {
                    //        var minionPathString = data.EntityList.EntitiesAsDictionary[minionId].Path;
                    //        if (!minionPathString.Contains("RemoteMine"))
                    //            count++;
                    //    }
                    //}

                    if (_minions.Count - _settingMinion >= Settings.NeedMines.Value && (_thread == null || !_run))
                    {
                        _thread = new Thread(Boom);
                        _thread.Start();
                    }
                }
                catch (Exception e)
                {
                    LogMessage(e.Message, 3);
                    LogMessage(e.Source, 3);
                    throw;
                }
            }
        }

        private void Boom()
        {
            var r = _random.Next(10, 50);
            _run = true;
            do
            {
                if (GameController.Game.IngameState.IngameUi.InventoryPanel.IsVisible
                    || GameController.Game.IngameState.IngameUi.OpenLeftPanel.IsVisible
                    || GameController.Game.IngameState.IngameUi.AtlasPanel.IsVisible
                    || GameController.Game.IngameState.IngameUi.OpenRightPanel.IsVisible)
                {
                    _run = false;
                    return;
                }

                if (_minions.Count - _settingMinion < Settings.NeedMines.Value)
                {
                    _run = false;
                    return;
                }

                try
                {
                    KeyTools.KeyEvent(Settings.DetonateKey.Value, KeyTools.KeyEventFlags.KeyEventKeyDown);
                    Thread.Sleep(64);
                    KeyTools.KeyEvent(Settings.DetonateKey.Value, KeyTools.KeyEventFlags.KeyEventKeyUp);
                    Thread.Sleep(Settings.Delay.Value + r);
                    //LogMessage(r, 5);
                }
                catch (Exception e)
                {
                    LogMessage(e.Message, 3);
                    LogMessage(e.Source, 3);
                    _run = false;
                    throw;
                }
            }
            while (_run);
        }
    }

    internal static class KeyTools
    {
        [DllImport("user32.dll", EntryPoint = "keybd_event", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern void keybd_event(byte vk, byte scan, int flags, int extrainfo);

        [Flags]
        public enum KeyEventFlags
        {
            KeyEventShiftVirtual = 0x10,
            KeyLControlVirtual = 0x11,
            KeyLeftVirtual = 0x25,
            KeyD = 0x44,
            KeyRightVirtual = 0x27,
            KeyEventKeyDown = 0,
            KeyEventKeyUp = 2,
        }

        public static void KeyEvent(Keys key, KeyEventFlags value)
        {
            keybd_event((byte) key, 0, (int) value, 0);
        }
    }
}