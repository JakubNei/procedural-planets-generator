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


        public bool disabledInput = false;

        public bool alignToEnabled;
        public Vector3 alignToPosition;

        Vector3 direction = new Vector3(1, 0, 0);
        Vector3 up = new Vector3(0, 1, 0);

        float cameraSpeed = 10.0f;
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

            Debug.AddValue("cameraSpeed", cameraSpeed.ToString());
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

            if (scrollWheelDelta > 0) cameraSpeed *= 1.3f;
            if (scrollWheelDelta < 0) cameraSpeed /= 1.3f;
            cameraSpeed = MyMath.Clamp(cameraSpeed, 1, 500);


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

            var rot =
                Quaternion.FromAxisAngle(Vector3.UnitX, pitch) *
                Quaternion.FromAxisAngle(Vector3.UnitY, yaw) *
                Quaternion.FromAxisAngle(Vector3.UnitZ, roll);

            up = up.RotateBy(rot);
            direction = direction.RotateBy(rot);


            float d = cameraSpeed * (float)deltaTime;

            if (Input.GetKey(Key.ShiftLeft)) d *= 5;

            var targetVelocity = Vector3.Zero;
            if (Input.GetKey(Key.W)) targetVelocity += d * Constants.Vector3Forward;
            if (Input.GetKey(Key.S)) targetVelocity -= d * Constants.Vector3Forward;
            if (Input.GetKey(Key.D)) targetVelocity += d * Constants.Vector3Right;
            if (Input.GetKey(Key.A)) targetVelocity -= d * Constants.Vector3Right;
            if (Input.GetKey(Key.Space)) targetVelocity += d * Constants.Vector3Up;
            if (Input.GetKey(Key.ControlLeft)) targetVelocity -= d * Constants.Vector3Up;

            //var pos = Matrix4.CreateTranslation(targetVelocity);


            var currentUp = this.up;
             
            if (alignToEnabled)
            {
                alignToPosition = new Vector3(1000, -100, 1000);
                currentUp = alignToPosition.Towards(Transform.Position).Normalized();
            }


            var mat = Matrix4.LookAt(Vector3.Zero, direction, currentUp);
            var currentRotation = mat.ExtractRotation();

            Entity.Transform.Rotation = currentRotation;

            targetVelocity = targetVelocity.RotateBy(Transform.Rotation);
            currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity, velocityChangeSpeed * (float)deltaTime);

            Transform.Position += currentVelocity;

            //Debug.Info(entity.transform.position);


        }

    }
}
