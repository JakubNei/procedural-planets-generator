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

		Quaternion rotation;

		float cameraSpeed = 10.0f;
		Point lastMousePos;
		int scrollWheelValue;
		Vector3 currentVelocity;

		float walkOnSphere_totalYaw;
		float walkOnSphere_totalPitch;

		bool WalkOnPlanet => Debug.CVar("walkOnPlanet").Bool;

		public FirstPersonCamera(Entity entity) : base(entity)
		{

			rotation = QuaternionUtility.LookRotation(Constants.Vector3Forward, Constants.Vector3Up);

			Input.LockCursor = disabledInput;

			Entity.EventSystem.Register<InputUpdate>(e => Update((float)e.DeltaTimeNow));

			Debug.CVar("walkOnPlanet").ToogledByKey(Key.G);
		}

		void Update(float deltaTime)
		{

			//Debug.AddValue("cameraSpeed", cameraSpeed.ToString());
			//Debug.AddValue("cameraPos", Transform.Position.ToString());

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


			if (Input.LockCursor == false) return;

			if (scrollWheelDelta > 0) cameraSpeed *= 1.3f;
			if (scrollWheelDelta < 0) cameraSpeed /= 1.3f;
			cameraSpeed = MyMath.Clamp(cameraSpeed, 1, 1000);


			/*
            var p = System.Windows.Forms.Cursor.Position;
            p.X -= mouseDelta.X;
            p.Y -= mouseDelta.Y;
            System.Windows.Forms.Cursor.Position = p;*/


			float d = cameraSpeed;

			if (Input.GetKey(Key.ShiftLeft)) d *= 5;

			var targetVelocity = Vector3.Zero;
			if (Input.GetKey(Key.W)) targetVelocity += d * Constants.Vector3Forward;
			if (Input.GetKey(Key.S)) targetVelocity -= d * Constants.Vector3Forward;
			if (Input.GetKey(Key.D)) targetVelocity += d * Constants.Vector3Right;
			if (Input.GetKey(Key.A)) targetVelocity -= d * Constants.Vector3Right;
			if (Input.GetKey(Key.Space)) targetVelocity += d * Constants.Vector3Up;
			if (Input.GetKey(Key.ControlLeft)) targetVelocity -= d * Constants.Vector3Up;

			//var pos = Matrix4.CreateTranslation(targetVelocity);


			float pitch = 0;
			float yaw = 0;
			float roll = 0;

			float c = 0.7f * (float)deltaTime;
			yaw += mouseDelta.X * c;
			pitch += mouseDelta.Y * c;

			if (Input.GetKey(Key.Q)) roll -= c;
			if (Input.GetKey(Key.E)) roll += c;


			var planet = PlanetaryBody.Root.instance;

			if (WalkOnPlanet)
			{

				walkOnSphere_totalYaw += yaw;
				walkOnSphere_totalPitch += pitch;

				var r = (float)Math.PI / 2.0f - 0.1f;
				if (walkOnSphere_totalPitch > r) walkOnSphere_totalPitch = r;
				if (walkOnSphere_totalPitch < -r) walkOnSphere_totalPitch = -r;

				var pointOnPlanet = planet.Center.Towards(this.Transform.Position).ToVector3d();
				var targetUp = pointOnPlanet.Normalized().ToVector3().Normalized();

				var s = planet.CalestialToSpherical(pointOnPlanet);
				s.latitude += 0.1f;
				var targetFwd = pointOnPlanet.Towards(planet.SphericalToCalestial(s)).Normalized().ToVector3().Normalized();

				var rotUp =
					Matrix4.LookAt(Vector3.Zero, targetFwd, targetUp).ExtractRotation().Inverted() *
					QuaternionUtility.FromEulerAngles(0, -walkOnSphere_totalYaw, -walkOnSphere_totalPitch);

				rotation = rotUp;

				/*
				 new Matrix3(
						targetRight,
						targetUp,
						targetFwd
					).ExtractRotation() *
				 */

				/*
				var planet = PlanetaryBody.Root.instance;

				var targetUp = planet.Center.Towards(this.Transform.Position).ToVector3d().Normalized().ToVector3().Normalized();
				var currentUp = Constants.Vector3Up.RotateBy(rotation).Normalized();

				Debug.AddValue("c.up", currentUp);
				Debug.AddValue("t.up", targetUp);


				var t = targetUp.Cross(currentUp);
				var a = targetUp.Angle(currentUp);


				{
					// possible
					var pot_r_delta = Quaternion.FromAxisAngle(t, a * 0.1f);
					var pot_rot = rotation * pot_r_delta;
					var pot_up = Constants.Vector3Up.RotateBy(pot_rot).Normalized();
					var pot_a = targetUp.Angle(pot_up);
					if (pot_a > a) a *= -1;
				}

				var r = Quaternion.FromAxisAngle(t, a * deltaTime);

				Debug.AddValue("a", a);

				rotation = rotation * r;

				*/
				//var targetRot = QuaternionUtility.LookRotation(targetFwd, targetUp);


				//alignToPosition = new Vector3(1000, -100, 1000);
				//currentUp = alignToPosition.Towards(Transform.Position).Normalized();
			}
			else
			{
				var rotDelta =
					Quaternion.FromAxisAngle(-Vector3.UnitX, pitch) *
					Quaternion.FromAxisAngle(-Vector3.UnitY, yaw) *
					Quaternion.FromAxisAngle(-Vector3.UnitZ, roll);


				rotation = rotation * rotDelta;


			}


			Entity.Transform.Rotation = rotation;

			targetVelocity = targetVelocity.RotateBy(Transform.Rotation);
			currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity, velocityChangeSpeed * (float)deltaTime);

			Transform.Position += currentVelocity * (float)deltaTime;

			var clampCameraToSurface = true;
			// make cam on top of the planet
			if (clampCameraToSurface)
			{
				var p = (Transform.Position - planet.Transform.Position).ToVector3d();
				var camPosS = planet.CalestialToSpherical(p);
				var h = 1 + planet.GetHeight(p);
				if (camPosS.altitude < h || WalkOnPlanet)
				{
					camPosS.altitude = h;
					Transform.Position = planet.Transform.Position + planet.SphericalToCalestial(camPosS).ToVector3();
				}
			}


			//Debug.Info(entity.transform.position);


		}

	}
}
