using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;
using OpenTK.Input;

using MyEngine;
using MyEngine.Components;

namespace MyGame
{
	public class DebugKeys
	{
		SceneSystem scene;
		Debug debug;

		public DebugKeys(SceneSystem scene, Debug debug)
		{
			this.scene = scene;
			this.debug = debug;
			scene.EventSystem.On<MyEngine.Events.InputUpdate>(e => Update(e.DeltaTime));
		}

		
		/*public static float keyTG = 0;
		public static float keyZH = 0;*/
		public static float keyUJ = 0;
		public static float keyIK = 0;
		//public static float keyOL = 0;

		void Update(double dt)
		{
			float s = 0.3f;
			
			/*var newKeyTG = keyTG;
			if (scene.Input.GetKey(Key.T)) newKeyTG += (float)dt * s;
			if (scene.Input.GetKey(Key.G)) newKeyTG -= (float)dt * s;
			MyMath.Clamp01(ref newKeyTG);
			if (newKeyTG != keyTG)
			{
				keyTG = newKeyTG;
				debug.Info("keyTG = " + keyTG);
			}

			var newKeyZH = keyZH;
			if (scene.Input.GetKey(Key.Z)) newKeyZH += (float)dt * s;
			if (scene.Input.GetKey(Key.H)) newKeyZH -= (float)dt * s;
			MyMath.Clamp01(ref newKeyZH);
			if (newKeyZH != keyZH)
			{
				keyZH = newKeyZH;
				debug.Info("keyZH = " + keyZH);
			}
			*/
			var newKeyUJ = keyUJ;
			if (scene.Input.GetKey(Key.U)) newKeyUJ += (float)dt * s;
			if (scene.Input.GetKey(Key.J)) newKeyUJ -= (float)dt * s;
			MyMath.Clamp01(ref newKeyUJ);
			if (newKeyUJ != keyUJ)
			{
				keyUJ = newKeyUJ;
				debug.Info("keyUJ = " + keyUJ);
			}

			var newKeyIK = keyIK;
			if (scene.Input.GetKey(Key.I)) newKeyIK += (float)dt * s;
			if (scene.Input.GetKey(Key.K)) newKeyIK -= (float)dt * s;
			MyMath.Clamp01(ref newKeyIK);
			if (newKeyIK != keyIK)
			{
				keyIK = newKeyIK;
				debug.Info("keyIK = " + keyIK);
			}
			/*
			var newKeyOL = keyOL;
			if (scene.Input.GetKey(Key.O)) newKeyOL += (float)dt * s;
			if (scene.Input.GetKey(Key.L)) newKeyOL -= (float)dt * s;
			MyMath.Clamp01(ref newKeyOL);
			if (newKeyOL != keyOL)
			{
				keyOL = newKeyOL;
				debug.Info("keyOL = " + keyOL);
			}			
			*/
		}
	}
}