using System;
using System.IO;
using System.Drawing;

using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using MyEngine;
using MyEngine.Events;
using MyEngine.Components;

namespace MyGame
{
    public class FirstPersonCamera : ComponentWithShortcuts
    {

        public float velocityChangeSpeed = 10.0f;

        private Vector3 up = Vector3.UnitY;

        public bool disabledInput = false;

        Matrix4 currentRotation = Matrix4.Identity;

        float speedModifier = 1.0f;
        Point lastMousePos;
        int scrollWheelValue;
        Vector3 currentVelocity;

        public FirstPersonCamera(Entity entity) : base(entity)
        {

            Input.LockCursor = disabledInput;

            Entity.EventSystem.Register<GraphicsUpdate>(e => Update(e.DeltaTime));

        }

        void Update(double deltaTime)
        {

            Debug.AddValue("cameraSpeed", speedModifier.ToString());
            Debug.AddValue("cameraPos", Transform.Position.ToString());

            var mouse = Mouse.GetState();

            var mouseDelta = new Point(mouse.X - lastMousePos.X, mouse.Y - lastMousePos.Y);
            lastMousePos = new Point(mouse.X, mouse.Y);

            int scrollWheelDelta = mouse.ScrollWheelValue - scrollWheelValue;
            scrollWheelValue = mouse.ScrollWheelValue;


            if (Input.GetKeyDown(Key.Escape))
            {
                if (Scene.Engine.WindowState == WindowState.Fullscreen)
                {
                    Scene.Engine.WindowState = WindowState.Normal;
                }
                else if (Input.LockCursor)
                {
                    Input.LockCursor = false;
                    Input.CursorVisible = true;
                }
                else
                {
                    Scene.Engine.Exit();
                }
            }

            if (Input.GeMouseButtonDown(MouseButton.Left))
            {
                if (Input.LockCursor == false)
                {
                    Input.LockCursor = true;
                    Input.CursorVisible = false;
                }
            }

            if (Input.GetKeyDown(Key.F))
            {
                if (Scene.Engine.WindowState != WindowState.Fullscreen)
                    Scene.Engine.WindowState = WindowState.Fullscreen;
                else
                    Scene.Engine.WindowState = WindowState.Normal;
            }


            if (Input.LockCursor == false) return;

            speedModifier += scrollWheelDelta;
            speedModifier = Math.Max(1.0f, speedModifier);


            /*
            var p = System.Windows.Forms.Cursor.Position;
            p.X -= mouseDelta.X;
            p.Y -= mouseDelta.Y;
            System.Windows.Forms.Cursor.Position = p;*/

            float pitch = 0;
            float yaw = 0;
            float roll = 0;


            float c = 1f * (float)deltaTime;
            yaw += mouseDelta.X * c;
            pitch += mouseDelta.Y * c;

            if (Input.GetKey(Key.Q)) roll -= c;
            if (Input.GetKey(Key.E)) roll += c;

            var rot = Matrix4.CreateFromQuaternion(
                Quaternion.FromAxisAngle(Entity.Transform.TransformDirection(Vector3.UnitY), -yaw) *
                Quaternion.FromAxisAngle(Entity.Transform.TransformDirection(Vector3.UnitX), -pitch) *
                Quaternion.FromAxisAngle(Entity.Transform.TransformDirection(Vector3.UnitZ), -roll)
            );

            currentRotation = currentRotation * rot;


            float d = speedModifier * (float)deltaTime;

            if (Input.GetKey(Key.ShiftLeft)) d *= 5;

            var targetVelocity = Vector3.Zero;
            if (Input.GetKey(Key.W)) targetVelocity.Z -= d;
            if (Input.GetKey(Key.S)) targetVelocity.Z += d;
            if (Input.GetKey(Key.D)) targetVelocity.X += d;
            if (Input.GetKey(Key.A)) targetVelocity.X -= d;
            if (Input.GetKey(Key.Space)) targetVelocity.Y += d;
            if (Input.GetKey(Key.ControlLeft)) targetVelocity.Y -= d;

            //var pos = Matrix4.CreateTranslation(targetVelocity);


            targetVelocity = Vector3.TransformPosition(targetVelocity, currentRotation);
            currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity, velocityChangeSpeed * (float)deltaTime);

            Entity.Transform.Rotation = currentRotation.ExtractRotation();
            Entity.Transform.Position += currentVelocity;

            //Debug.Info(entity.transform.position);


        }

    }
}
