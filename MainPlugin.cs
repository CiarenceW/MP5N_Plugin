using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;

namespace MP5_plugin
{
    [BepInDependency("pl.szikaka.receiver_2_modding_kit")]
    [BepInPlugin("Ciarencew.MP5N", "MP5N Plugin", "2.0.0")]
    internal class MainPlugin : BaseUnityPlugin
    {
        public static ConfigEntry<int> stock_setting;
        public static MainPlugin instance
        {
            get;
            private set;
        }

        public static readonly string folder_name = "MP5N_Files";
        private void Awake()
        {
            Logger.LogInfo("MP5N Main Plugin loaded!");

            stock_setting = Config.Bind("MP5 Setting", "Stock option", 1, "What stock to choose, hmmmmmmmm... so many choices!");

            instance = this;
        }
    }

}
