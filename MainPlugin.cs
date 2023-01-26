using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;

namespace MP5_plugin
{
    [BepInDependency("pl.szikaka.receiver_2_modding_kit")]
    [BepInPlugin("Ciarencew.MP5N", "MP5N Plugin", "2.0.0")]
    internal class MainPlugin : BaseUnityPlugin
    {
        public static MainPlugin instance
        {
            get;
            private set;
        }

        public static readonly string folder_name = "MP5N_Files";
        private void Awake()
        {
            Logger.LogInfo("MP5N Main Plugin loaded!");

            instance = this;
        }
    }

}
