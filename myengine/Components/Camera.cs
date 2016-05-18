using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using MyEngine;
using MyEngine.Components;


namespace MyEngine.Components
{    

    public class Camera : ComponentWithShortcuts
    {

        public float aspect = 0.8f;
        public float fieldOfView = 45.0f;
        public float nearClipPlane = 0.1f;
        public float farClipPlane = 5000;
        public bool orthographic = false;
        public float orthographicSize = 5;
        public int pixelWidth;
        public int pixelHeight;
        Vector2 screenSize = Vector2.Zero;

        public WorldPos ViewPointPosition
        {
            get
            {
                return this.Entity.Transform.Position;
            }
        }

        public Vector3 ambientColor = Vector3.One * 0.1f;

        public List<IPostProcessEffect> postProcessEffects = new List<IPostProcessEffect>();

        public Camera(Entity entity) : base(entity)
        {
            entity.EventSystem.Register<Events.WindowResized>(e => SetSize(e.NewPixelWidth, e.NewPixelHeight));
        }

        public void SetSize(int newPixelWidth, int newPixelHeight) {
            this.pixelHeight = newPixelHeight;
            this.pixelWidth = newPixelWidth;
            screenSize = new Vector2(newPixelWidth, newPixelHeight);
            aspect = screenSize.X / screenSize.Y;
        }

        public Matrix4 GetProjectionMat()
        {
            if (orthographic) return Matrix4.CreateOrthographic(orthographicSize * 2, orthographicSize * 2 / aspect, nearClipPlane, farClipPlane);
            return Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 180.0f * fieldOfView, aspect, nearClipPlane, farClipPlane);
        }
        

        public Matrix4 GetRotationMatrix()
        {
            return
                Matrix4.CreateFromQuaternion(Quaternion.Invert(Transform.Rotation));
        }

        public Vector3 GetScreenPos(WorldPos worldPos)
        {
            var mp = GetRotationMatrix() * GetProjectionMat();
            var p = Vector4.Transform(new Vector4(this.Transform.Position.Towards(worldPos).ToVector3(), 1), mp);
            return (p.Xyz / p.W) / 2 + Vector3.One / 2;
        }

        public void UploadDataToUBO(UniformBlock ubo)
        {
            ubo.engine.viewMatrix = GetRotationMatrix();
            ubo.engine.projectionMatrix = GetProjectionMat();
            ubo.engine.viewProjectionMatrix = ubo.engine.viewMatrix * ubo.engine.projectionMatrix;
            ubo.engine.screenSize = this.screenSize;
            ubo.engine.nearClipPlane = this.nearClipPlane;
            ubo.engine.farClipPlane = this.farClipPlane;
            ubo.engine.ambientColor = this.ambientColor;
            GL.Viewport(0, 0, pixelWidth, pixelHeight);
            ubo.engineUBO.UploadData();
        }

        public void AddPostProcessEffect(IPostProcessEffect shader)
        {
            postProcessEffects.Add(shader);
        }

        /// <summary>
        /// Returns frustum planes of rotation and projection matrix
        /// </summary>
        /// <returns></returns>
        public Plane[] GetFrustumPlanes()
        {
            var p = new Plane[6];
            var m = GetRotationMatrix() * GetProjectionMat();

            const int FRUSTUM_RIGHT = 0;
            const int FRUSTUM_LEFT = 1;
            const int FRUSTUM_DOWN = 2;
            const int FRUSTUM_UP = 3;
            const int FRUSTUM_FAR = 4;
            const int FRUSTUM_NEAR = 5;

            p[FRUSTUM_RIGHT].normal.X = m[0, 3] - m[0, 0];
            p[FRUSTUM_RIGHT].normal.Y = m[1, 3] - m[1, 0];
            p[FRUSTUM_RIGHT].normal.Z = m[2, 3] - m[2, 0];
            p[FRUSTUM_RIGHT].distance = m[3, 3] - m[3, 0];

            p[FRUSTUM_LEFT].normal.X = m[0, 3] + m[0, 0];
            p[FRUSTUM_LEFT].normal.Y = m[1, 3] + m[1, 0];
            p[FRUSTUM_LEFT].normal.Z = m[2, 3] + m[2, 0];
            p[FRUSTUM_LEFT].distance = m[3, 3] + m[3, 0];

            p[FRUSTUM_DOWN].normal.X = m[0, 3] + m[0, 1];
            p[FRUSTUM_DOWN].normal.Y = m[1, 3] + m[1, 1];
            p[FRUSTUM_DOWN].normal.Z = m[2, 3] + m[2, 1];
            p[FRUSTUM_DOWN].distance = m[3, 3] + m[3, 1];

            p[FRUSTUM_UP].normal.X = m[0, 3] - m[0, 1];
            p[FRUSTUM_UP].normal.Y = m[1, 3] - m[1, 1];
            p[FRUSTUM_UP].normal.Z = m[2, 3] - m[2, 1];
            p[FRUSTUM_UP].distance = m[3, 3] - m[3, 1];

            p[FRUSTUM_FAR].normal.X = m[0, 3] - m[0, 2];
            p[FRUSTUM_FAR].normal.Y = m[1, 3] - m[1, 2];
            p[FRUSTUM_FAR].normal.Z = m[2, 3] - m[2, 2];
            p[FRUSTUM_FAR].distance = m[3, 3] - m[3, 2];

            p[FRUSTUM_NEAR].normal.X = m[0, 3] + m[0, 2];
            p[FRUSTUM_NEAR].normal.Y = m[1, 3] + m[1, 2];
            p[FRUSTUM_NEAR].normal.Z = m[2, 3] + m[2, 2];
            p[FRUSTUM_NEAR].distance = m[3, 3] + m[3, 2];

            return p;
        }
    }
}
