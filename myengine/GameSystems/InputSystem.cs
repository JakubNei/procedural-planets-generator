using System;
using System.Collections.Generic;
using System.Text;



using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;


namespace MyEngine
{
    public class InputSystem : GameSystemBase
    {

        public bool CursorVisible
        {
            get
            {
                return engine.CursorVisible;
            }
            set
            {
                engine.CursorVisible = value;
            }
        }

        public bool LockCursor
        {
            get
            {
                return CursorVisible == false;
            }
            set
            {
                CursorVisible = value == false;
            }
        }

        static Dictionary<Key, bool> isDown = new Dictionary<Key, bool>();
        static Dictionary<Key, bool> releasedThisFrame = new Dictionary<Key, bool>();
        static Dictionary<Key, bool> pressedThisFrame = new Dictionary<Key, bool>();

        static Dictionary<MouseButton, bool> isDown_mouseButton = new Dictionary<MouseButton, bool>();
        static Dictionary<MouseButton, bool> releasedThisFrame_mouseButton = new Dictionary<MouseButton, bool>();
        static Dictionary<MouseButton, bool> pressedThisFrame_mouseButton = new Dictionary<MouseButton, bool>();

        EngineMain engine;

        public InputSystem(EngineMain engine)
        {
            this.engine = engine;
        }

        internal void Update()
        {
            var keyboard = Keyboard.GetState();
            foreach (Key k in System.Enum.GetValues(typeof(Key)))
            {
                bool newIsDown = keyboard.IsKeyDown(k);
                bool oldIsDown = false;
                isDown.TryGetValue(k, out oldIsDown);

                if (oldIsDown == false && newIsDown == true) pressedThisFrame[k] = true;
                else pressedThisFrame[k] = false;

                if (oldIsDown == true && newIsDown == false) releasedThisFrame[k] = true;
                else releasedThisFrame[k] = false;

                isDown[k] = newIsDown;
            }

            var mouse = Mouse.GetState();
            foreach (MouseButton k in System.Enum.GetValues(typeof(MouseButton)))
            {
                bool newIsDown = mouse.IsButtonDown(k);
                bool oldIsDown = false;
                isDown_mouseButton.TryGetValue(k, out oldIsDown);

                if (oldIsDown == false && newIsDown == true) pressedThisFrame_mouseButton[k] = true;
                else pressedThisFrame_mouseButton[k] = false;

                if (oldIsDown == true && newIsDown == false) releasedThisFrame_mouseButton[k] = true;
                else releasedThisFrame_mouseButton[k] = false;

                isDown_mouseButton[k] = newIsDown;
            }

            if(LockCursor)
            {
                // todo
            }
        }

        public bool GetKey(Key k)
        {
            bool b = false;
            isDown.TryGetValue(k, out b);
            return b;
        }
        public bool GetKeyDown(Key k)
        {
            bool b = false;
            pressedThisFrame.TryGetValue(k, out b);
            return b;
        }
        public bool GetKeyUp(Key k)
        {
            bool b = false;
            releasedThisFrame.TryGetValue(k, out b);
            return b;
        }

        public bool GeMouseButton(MouseButton k)
        {
            bool b = false;
            isDown_mouseButton.TryGetValue(k, out b);
            return b;
        }
        public bool GeMouseButtonDown(MouseButton k)
        {
            bool b = false;
            pressedThisFrame_mouseButton.TryGetValue(k, out b);
            return b;
        }
        public bool GeMouseButtonUp(MouseButton k)
        {
            bool b = false;
            releasedThisFrame_mouseButton.TryGetValue(k, out b);
            return b;
        }
    }
}
