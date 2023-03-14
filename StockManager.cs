using BepInEx;
using BepInEx.Configuration;
using Receiver2ModdingKit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MP5_plugin
{
    public class StockManager : MonoBehaviour
    {
        StockScript[] stocks;
        private ModGunScript gun;
        public StockScript current_stock;

        public void InitializeManager()
        {// Create a new configuration file.
         // First argument is the path to where the configuration is saved
         // Second arguments specifes whether to create the file right away or whether to wait until any values are accessed/written
            var customFile = new ConfigFile(Path.Combine(Paths.ConfigPath, gun.InternalName+"_stock_config.cfg"), true);

            // You can now create configuration wrappers for it
            var userName = customFile.Bind("Stocks",
                "EquippedStock",
                0,
                "Name of the currently equipped stock.");
        }

        private void InitializeMoversStock()
        {
            foreach (StockScript stock in stocks)
            {
                if (stock.movable_stock)
                {
                    stock.mover.positions[0] = transform.Find(stock.stock_name + "_folded").localPosition;
                    stock.mover.positions[1] = transform.Find(stock.stock_name + "_unfolded").localPosition;
                }
            }
        }
        private void UpdateGunStats()
        {
            transform.Find("pose_aim_down_sights").localPosition = transform.Find(current_stock.stock_name + "/pose_ads").localPosition;
            gun.rotation_transfer_x_max = current_stock.recoil_trans_x_max;
            gun.rotation_transfer_x_min = current_stock.recoil_trans_x_min;
            gun.rotation_transfer_y_max = current_stock.recoil_trans_y_max;
            gun.rotation_transfer_y_min = current_stock.recoil_trans_y_min;
            gun.sway_multiplier = current_stock.shake_amount;
            gun.mass = gun.mass + current_stock.extra_mass;
        }
    }
}
