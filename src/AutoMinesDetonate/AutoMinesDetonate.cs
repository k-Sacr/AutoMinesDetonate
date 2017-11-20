using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using PoeHUD.Models;
using PoeHUD.Plugins;
using PoeHUD.Poe.Components;

namespace AutoMinesDetonate
{
    public class AutoMinesDetonate : BaseSettingsPlugin<AutoMinesDetonateSettings>
    {
        //private Stack<EntityWrapper> _mines = new Stack<EntityWrapper>();
        //private Dictionary<EntityWrapper, int> _mines = new Dictionary<EntityWrapper, int>();
        private List<int> _mines;

        private Thread _thread;
        private readonly Random _random = new Random();

        private bool _run;

        //private List<EntityWrapper> _minions = new List<EntityWrapper>();
        private HashSet<EntityWrapper> _activeTotems = new HashSet<EntityWrapper>();

        //GameController.Area.CurrentArea.IsTown
        public override void EntityAdded(EntityWrapper entity)
        {
            //if (Settings.Enable && entity != null && !_minions.Contains(entity) && entity.HasComponent<Monster>() && !entity.IsHostile)
            //{
            //    _minions.Add(entity);
            //}
            if (entity != null && entity.Path.Contains("StrengthTotem") && !entity.IsHostile)
            {
                _activeTotems.Add(entity);
            }
        }

        public override void EntityRemoved(EntityWrapper entity)
        {
            //_minions.Remove(entity);
            if (entity != null && entity.Path.Contains("StrengthTotem") && !entity.IsHostile)
                _activeTotems.Remove(entity);
            var minionPathString = "";
            foreach (var minionId in GameController.Game.IngameState.Data.LocalPlayer.GetComponent<Actor>().Minions.ToList())
            {
                if (GameController.Game.IngameState.Data.EntityList.EntitiesAsDictionary.ContainsKey(minionId))
                {
                    minionPathString = GameController.Game.IngameState.Data.EntityList.EntitiesAsDictionary[minionId].Path;
                    LogMessage(minionPathString, 3);
                }
            }
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

                    _mines = GameController.Game.IngameState.Data.LocalPlayer.GetComponent<Actor>().Minions;

                    //if (_mines >= Settings.NeedMines.Value)
                    //{
                    //    try
                    //    {
                    //        KeyTools.KeyEvent(KeyTools.KeyEventFlags.KeyD, KeyTools.KeyEventFlags.KeyEventKeyDown);
                    //        Thread.Sleep(64);
                    //        KeyTools.KeyEvent(KeyTools.KeyEventFlags.KeyD, KeyTools.KeyEventFlags.KeyEventKeyUp);
                    //        Thread.Sleep(Settings.Delay.Value + _random.Next(10, 50));
                    //    }
                    //    catch (Exception e)
                    //    {
                    //        LogMessage(e.Message, 3);
                    //        LogMessage(e.Source, 3);
                    //        throw;
                    //    }
                    //}

                    if (_mines.Count - Settings.Minions.Value >= Settings.NeedMines.Value)
                    {
                        if (_thread == null || !_run)
                        {
                            _thread = new Thread(Boom);
                            _thread.Start();
                            //LogMessage("Поток:" + _thread.ManagedThreadId, 5);

                            Thread.Sleep(200);
                        }
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
            int minion = 0;
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

                minion += (from minionId in _mines where GameController.Game.IngameState.Data.EntityList.EntitiesAsDictionary.ContainsKey(minionId) select GameController.Game.IngameState.Data.EntityList.EntitiesAsDictionary[minionId].Path).Count(minionPathString => !minionPathString.Contains("RemoteMine"));

                if (_mines.Count - minion < Settings.NeedMines.Value)
                {
                    _run = false;
                    return;
                }

                

                try
                {
                    KeyTools.KeyEvent(KeyTools.KeyEventFlags.KeyD, KeyTools.KeyEventFlags.KeyEventKeyDown);
                    Thread.Sleep(64);
                    KeyTools.KeyEvent(KeyTools.KeyEventFlags.KeyD, KeyTools.KeyEventFlags.KeyEventKeyUp);
                    Thread.Sleep(Settings.Delay.Value + r);
                    //LogMessage(r, 5);
                    //foreach (var mine in _mines.ToList())
                    //{
                    //    if (mine.Value > 1)
                    //        _mines[mine.Key] = mine.Value - 1;
                    //    else
                    //        _mines.Remove(mine.Key);
                    //}
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

        public static void KeyEvent(KeyEventFlags key, KeyEventFlags value)
        {
            keybd_event((byte) key,
                0,
                (int) value,
                0);
        }
    }
}