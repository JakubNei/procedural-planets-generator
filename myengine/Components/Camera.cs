using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using MyEngine;

namespace MyEngine.Components
{
    public class Camera : ComponentWithShortcuts
    {

        public static Camera main { get; internal set; }

        public float aspect = 0.8f;
        public float fieldOfView = 45.0f;
        public float nearClipPlane = 0.1f;
        public float farClipPlane = 5000;
        public bool orthographic = false;
        public float orthographicSize = 5;
        public int pixelWidth;
        public int pixelHeight;
        Vector2 screenSize = Vector2.Zero;

        public Vector3 ambientColor = new Vector3(0.2f,0.2f,0.2f);

        public List<Shader> postProcessEffects = new List<Shader>();

        public Camera(Entity entity) : base(entity)
        {
        }

        public void SetSize(int w, int h) {
            this.pixelHeight = h;
            this.pixelWidth = w;
            screenSize = new Vector2(w, h);
            aspect = screenSize.X / screenSize.Y;
        }

        public Matrix4 GetProjectionMat()
        {
            if (orthographic) return Matrix4.CreateOrthographic(orthographicSize * 2, orthographicSize * 2 / aspect, nearClipPlane, farClipPlane);
            return Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 180.0f * fieldOfView, aspect, nearClipPlane, farClipPlane);
        }
        

        public Matrix4 GetViewMat()
        {
            return
                Matrix4.CreateTranslation(-Transform.Position) *
                Matrix4.CreateFromQuaternion(Quaternion.Invert(Transform.Rotation));
        }

        public void UploadDataToUBO(UniformBlock ubo)
        {
            ubo.engine.viewMatrix = GetViewMat();
            ubo.engine.projectionMatrix = GetProjectionMat();
            ubo.engine.viewProjectionMatrix = ubo.engine.viewMatrix * ubo.engine.projectionMatrix;
            ubo.engine.cameraPosition = this.Entity.Transform.Position;
            ubo.engine.screenSize = this.screenSize;
            ubo.engine.nearClipPlane = this.nearClipPlane;
            ubo.engine.farClipPlane = this.farClipPlane;
            ubo.engine.ambientColor = this.ambientColor;
            GL.Viewport(0, 0, pixelWidth, pixelHeight);
            ubo.engineUBO.UploadData();
        }

        public void AddPostProcessEffect(Shader shader)
        {
            postProcessEffects.Add(shader);
        }

        public Plane[] GetFrustumPlanes()
        {
            var p = new Plane[6];
            var m = GetViewMat() * GetProjectionMat();

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
