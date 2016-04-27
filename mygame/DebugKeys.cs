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

        public DebugKeys(SceneSystem scene)
        {
            this.scene = scene;
            scene.EventSystem.Register((MyEngine.Events.GraphicsUpdate e) => OnGraphicsUpdate(e.DeltaTime));
        }


        public static float keyTG = 0;
        public static float keyZH = 0;
        public static float keyUJ = 0;
        public static float keyIK = 0;
        public static float keyOL = 0;

        void OnGraphicsUpdate(double dt)
        {
            float s = 0.3f;

            if (scene.Input.GetKey(Key.T)) keyTG += (float)dt * s;
            if (scene.Input.GetKey(Key.G)) keyTG -= (float)dt * s;
            MyMath.Clamp01(ref keyTG);

            if (scene.Input.GetKey(Key.Z)) keyZH += (float)dt * s;
            if (scene.Input.GetKey(Key.H)) keyZH -= (float)dt * s;
            MyMath.Clamp01(ref keyZH);

            if (scene.Input.GetKey(Key.U)) keyUJ += (float)dt * s;
            if (scene.Input.GetKey(Key.J)) keyUJ -= (float)dt * s;
            MyMath.Clamp01(ref keyUJ);

            if (scene.Input.GetKey(Key.I)) keyIK += (float)dt * s;
            if (scene.Input.GetKey(Key.K)) keyIK -= (float)dt * s;
            MyMath.Clamp01(ref keyIK);

            if (scene.Input.GetKey(Key.O)) keyOL += (float)dt * s;
            if (scene.Input.GetKey(Key.L)) keyOL -= (float)dt * s;
            MyMath.Clamp01(ref keyOL);

        }
    }
}