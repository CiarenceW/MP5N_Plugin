using System;
using System.Linq;
using System.Collections.Generic;
using Receiver2;
using Receiver2ModdingKit;
using UnityEngine;
using RewiredConsts;
using System.Reflection;

namespace MP5_plugin
{
    public class MP5Script : ModGunScript
	{
		//thanks Szikaka for having already done the code of the worse version of this gun, lol
		private float slide_forward_speed = -8;
		private float hammer_accel = -5000;
		private float m_charging_handle_amount;
		private int fired_bullet_count;
		private float safety_held_time;

		private readonly float[] slide_push_hammer_curve = new float[] {
			0,
			0,
			0.2f,
			1
		};
		private ModHelpEntry help_entry;
		public Sprite help_entry_sprite;
		public override ModHelpEntry GetGunHelpEntry()
		{
			return help_entry = new ModHelpEntry("HK MP5")
			{
				info_sprite = this.help_entry_sprite,
				title = "H&K MP5",
				description = "Heckler & Koch MP5-N, HK MP5, MP5A4\n"
							+ "Capacity: 30 + 1, 9x19mm NATO\n"
							+ "\n"
							+ "Following the success of the H&K G3 battle rifle, the engineers at Heckler & Koch developed a family of small arms all based around the same G3 base design and delayed blowback operating system. Originally internally known as the HK54, it became known as the MP5 after having been adopted by the German Federal Police in 1966. Later, H&K introduced the MP5A1 model, and with it, the iconic ring front sight and slimline handguard.\n"
							+ "\n"
							+ "In the 1983, the MP5-N was released, designed specifically for use by the US Navy, and later adopted by other countries. It features a threaded barrel, fixed titanium front sights, and a Navy trigger group with a Safe, Semi, Burst, and Auto fire mode."
			};
		}
		public override LocaleTactics GetGunTactics()
		{
			return new LocaleTactics()
			{
				gun_internal_name = InternalName,
				title = "H&K MP5-N\n",
				text = "A modded SMG, made while listening to Jus†ice on repeat\n" +
					   "A variant of the MP5 made for the United States Navy. Featuring a three fire mode Navy trigger group, including safe, single fire, 2-3 round burst, and fully automatic"
			};
		}
		public override void InitializeGun()
		{
			pooled_muzzle_flash = ((GunScript)ReceiverCoreScript.Instance().generic_prefabs.First(it => { return it is GunScript && ((GunScript)it).gun_model == GunModel.BerettaM9; })).pooled_muzzle_flash;
			//loaded_cartridge_prefab = ((GunScript)ReceiverCoreScript.Instance().generic_prefabs.First(it => { return it is GunScript && ((GunScript)it).gun_model == GunModel.BerettaM9; })).loaded_cartridge_prefab;
		}
		public override void AwakeGun()
		{
			hammer.amount = 1;
		}
		public override void UpdateGun()
		{
			firing_modes[0].sound_event_path = sound_safety_on;
			firing_modes[1].sound_event_path = sound_safety_off;
			firing_modes[2].sound_event_path = sound_safety_off;
			firing_modes[3].sound_event_path = sound_safety_off;

			if (slide.amount == 0 && magazine.amount != 0) //checks if the mag is inserted, and if the slide is closed
            {
				force_wrongly_seated_mag = true;//if it's the case, makes the mag wrongly seated, to force the player to slap the mag, (kinda like in Ground Branch), to simulate the issues that come with inserting a mag with a chambered round in a real MP5.
            }
			else if (slide_stop.amount == 1) //if the slide is fully and locked, remove the flags
            {
				force_wrongly_seated_mag = false;
				mag_seated_wrong = false;
            }

			if (_select_fire.amount == 0.67f && !_disconnector_needs_reset) //burst fire logic, checks if the select fire component is at the burst fire value, and if the disconnector doesn't need a reset.
            {
				_disconnector_needs_reset = fired_bullet_count >= 3;
			}
            if (slide.amount > 0f && trigger.amount > 0f && _select_fire.amount < 1f) //prevents the gun from going off after racking the slide back on non-auto fire modes when the trigger is held
            {
                _disconnector_needs_reset = true;
            }

            hammer.asleep = true;
			hammer.accel = hammer_accel;

			if (slide.amount > 0 && _hammer_state != 3)
			{ // Bolt cocks the hammer when moving back 
				hammer.amount = Mathf.Max(hammer.amount, InterpCurve(slide_push_hammer_curve, slide.amount));
			}

			if (hammer.amount == 1) _hammer_state = 3;

			if (trigger.amount == 0) //checks if the trigger is not being pressed
            {
				fired_bullet_count = 0;
                _disconnector_needs_reset = false;  //if so, mark the disconnector as having been reset.
			}

            if (!IsSafetyOn())
            {
                if (player_input.GetButton(RewiredConsts.Action.Toggle_Safety_Auto_Mod))
                {
                    safety_held_time += Time.deltaTime;
                }
                else
                {
                    safety_held_time = 0;
                }

                if (safety_held_time >= 0.4f)
                {
                    SwitchFireMode();
                }
            }
            else
            {
                safety_held_time = 0;
            }
            if (IsSafetyOn())
			{ // Safety blocks the trigger from moving
				trigger.amount = Mathf.Min(trigger.amount, 0.1f);

				trigger.UpdateDisplay();
			}

			if (_hammer_state != 3 && ((trigger.amount == 1 && !_disconnector_needs_reset && slide.amount == 0) || hammer.amount != _hammer_cocked_val))
			{ // Move hammer if it's cocked and is free to move
				hammer.asleep = false;
			}

			if (slide.amount == 0 && _hammer_state == 3 && trigger.amount == 1)
			{ // Simulate auto sear
				hammer.amount = Mathf.MoveTowards(hammer.amount, _hammer_cocked_val, Time.deltaTime * Time.timeScale * 50);
				if (hammer.amount == _hammer_cocked_val) _hammer_state = 2;
			}

			hammer.TimeStep(Time.deltaTime);

			/*if (player_input.GetButton(RewiredConsts.Action.Toggle_Safety_Auto_Mod)) //safety held logic
            {
				safety_held_down += Time.deltaTime;
				if (safety_held_down > 0.5 && (int)current_firing_mode_index.GetValue(this) != 0) //if the safety key is being held for more than 0.5 sec, forces the safety to be down
                {
					current_firing_mode_index.SetValue(this, 0);
					AudioManager.PlayOneShotAttached(sound_safety_on, this.gameObject);
                }
            }
			else
            {
				safety_held_down = 0;
            }*/

			if (hammer.amount == 0 && _hammer_state == 2)
			{ // If hammer dropped and hammer was cocked then fire gun and decock hammer
				TryFireBullet(1, FireBullet);

				_disconnector_needs_reset = _select_fire.amount < 0.67f;

				if (_select_fire.amount == 0.67f) fired_bullet_count++;
				else fired_bullet_count = 0;

				_hammer_state = 0;
			}
			if (slide.vel < 0) slide.vel = Mathf.Max(slide.vel, slide_forward_speed); // Slow down the slide moving forward, reducing fire rate

			if (player_input.GetButton(RewiredConsts.Action.Pull_Back_Slide) || player_input.GetButtonUp(RewiredConsts.Action.Pull_Back_Slide))
			{
				m_charging_handle_amount = slide.amount;
			}
			else
			{
				m_charging_handle_amount = Mathf.Min(m_charging_handle_amount, slide.amount);
			}
			hammer.UpdateDisplay();

			ApplyTransform("charging_handle", m_charging_handle_amount, slide_stop.transform);
			ApplyTransform("magazine_release_button", magazine_catch.amount, transform.Find("magazine_release_button"));
			ApplyTransform("extractor_ejector", slide.amount, transform.Find("extractor_ejector"));

			trigger.UpdateDisplay();

			UpdateAnimatedComponents();
		}
	}
}
