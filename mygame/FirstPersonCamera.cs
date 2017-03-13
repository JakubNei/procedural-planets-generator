using System;
using System.IO;
using System.Drawing;

using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

using MyEngine;
using MyEngine.Events;
using MyEngine.Components;

namespace MyGame
{
	public class FirstPersonCamera : ComponentWithShortcuts
	{

		public float velocityChangeSpeed = 10.0f;
		public float mouseSensitivty = 0.3f;

		public bool disabledInput = false;
		public bool speedBasedOnDistanceToPlanet = true;
		public bool collideWithPlanetSurface = true;

		Quaternion rotation;
		WorldPos position;

		float cameraSpeedModifier = 10.0f;
		Point lastMousePos;
		int scrollWheelValue;
		Vector3 currentVelocity;

		Vector3 walkOnSphere_lastVectorUp;
		Vector3 walkOnSphere_vectorForward;
		bool walkOnShere_start;

		CVar WalkOnPlanet => Debug.GetCVar("walkOnPlanet");

		public FirstPersonCamera(Entity entity) : base(entity)
		{

			rotation = QuaternionUtility.LookRotation(Constants.Vector3Forward, Constants.Vector3Up);

			Input.LockCursor = disabledInput;

			Entity.EventSystem.Register<InputUpdate>(e => Update((float)e.DeltaTime));

			WalkOnPlanet.ToogledByKey(Key.G).OnChanged += (cvar) =>
			{
				if (cvar.Bool)
				{
					walkOnShere_start = true;
				}
			};
		}

		void Update(float deltaTime)
		{

			var planet = PlanetaryBody.Root.instance;


			Debug.AddValue("camera / speed modifier", cameraSpeedModifier);
			Debug.AddValue("camera / position", Transform.Position);

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

			if (scrollWheelDelta > 0) cameraSpeedModifier *= 1.3f;
			if (scrollWheelDelta < 0) cameraSpeedModifier /= 1.3f;
			cameraSpeedModifier = MyMath.Clamp(cameraSpeedModifier, 1, 100000);


			/*
            var p = System.Windows.Forms.Cursor.Position;
            p.X -= mouseDelta.X;
            p.Y -= mouseDelta.Y;
            System.Windows.Forms.Cursor.Position = p;*/


			float currentSpeed = cameraSpeedModifier;

			if (speedBasedOnDistanceToPlanet)
			{
				var planetLocalPosition = planet.Transform.Position.Towards(position).ToVector3d();
				var sphericalPlanetLocalPosition = planet.CalestialToSpherical(planetLocalPosition);
				var onPlanetSurfaceHeight = planet.GetSurfaceHeight(planetLocalPosition);
				var onPlanetDistanceToSurface = sphericalPlanetLocalPosition.altitude - onPlanetSurfaceHeight;
				Debug.AddValue("camera / distance to surface", onPlanetDistanceToSurface);

				onPlanetDistanceToSurface = MyMath.Clamp(onPlanetDistanceToSurface, 1, 10000);
				currentSpeed *= (1 + (float)onPlanetDistanceToSurface / 5.0f);
			}

			if (Input.GetKey(Key.ShiftLeft)) currentSpeed *= 5;

			Debug.AddValue("camera / real speed", currentSpeed);

			var targetVelocity = Vector3.Zero;
			if (Input.GetKey(Key.W)) targetVelocity += currentSpeed * Constants.Vector3Forward;
			if (Input.GetKey(Key.S)) targetVelocity -= currentSpeed * Constants.Vector3Forward;
			if (Input.GetKey(Key.D)) targetVelocity += currentSpeed * Constants.Vector3Right;
			if (Input.GetKey(Key.A)) targetVelocity -= currentSpeed * Constants.Vector3Right;
			if (Input.GetKey(Key.Space)) targetVelocity += currentSpeed * Constants.Vector3Up;
			if (Input.GetKey(Key.ControlLeft)) targetVelocity -= currentSpeed * Constants.Vector3Up;

			//var pos = Matrix4.CreateTranslation(targetVelocity);


			float pitchDelta = 0;
			float yawDelta = 0;
			float rollDelta = 0;

			float c = mouseSensitivty * (float)deltaTime;
			yawDelta += mouseDelta.X * c;
			pitchDelta += mouseDelta.Y * c;

			if (Input.GetKey(Key.Q)) rollDelta -= c;
			if (Input.GetKey(Key.E)) rollDelta += c;


			if (Input.GetKeyDown(Key.C))
			{
				rotation = this.Transform.Position.Towards(planet.Transform.Position).ToVector3().LookRot();
			}

			if (WalkOnPlanet.Bool)
			{

				var up = planet.Center.Towards(this.Transform.Position).ToVector3().Normalized();
				var fwd = walkOnSphere_vectorForward;

				if (walkOnShere_start)
				{
					walkOnSphere_lastVectorUp = up;

					var pointOnPlanet = planet.Center.Towards(this.Transform.Position).ToVector3d();
					var s = planet.CalestialToSpherical(pointOnPlanet);
					s.latitude += 0.1f;
					var fwdToPole = pointOnPlanet.Towards(planet.SphericalToCalestial(s)).Normalized().ToVector3().Normalized();

					fwd = Constants.Vector3Forward.RotateBy(rotation);
				}

				if (!walkOnShere_start)
				{
					var upDeltaAngle = up.Angle(walkOnSphere_lastVectorUp);
					var upDeltaRot = Quaternion.FromAxisAngle(up.Cross(walkOnSphere_lastVectorUp), upDeltaAngle).Inverted();

					fwd = fwd.RotateBy(upDeltaRot);
				}


				var left = up.Cross(fwd);

				var rotDelta =
					Quaternion.FromAxisAngle(up, -yawDelta) *
					Quaternion.FromAxisAngle(left, pitchDelta);


				fwd = fwd.RotateBy(rotDelta);

				{
					// clamping up down rotation
					var maxUpDownAngle = 80;
					var minUp = MyMath.ToRadians(90 - maxUpDownAngle);
					var maxDown = MyMath.ToRadians(90 + maxUpDownAngle);
					var angle = fwd.Angle(up);
					if (angle < minUp)
						fwd = up.RotateBy(Quaternion.FromAxisAngle(left, minUp));
					else if (angle > maxDown)
						fwd = up.RotateBy(Quaternion.FromAxisAngle(left, maxDown));
				}


				fwd.Normalize();
				up.Normalize();

				rotation = QuaternionUtility.LookRotation(fwd, up);

				walkOnSphere_vectorForward = fwd;
				walkOnSphere_lastVectorUp = up;
				walkOnShere_start = false;

			}
			else
			{
				var rotDelta =
					Quaternion.FromAxisAngle(-Vector3.UnitX, pitchDelta) *
					Quaternion.FromAxisAngle(-Vector3.UnitY, yawDelta) *
					Quaternion.FromAxisAngle(-Vector3.UnitZ, rollDelta);


				rotation = rotation * rotDelta;


			}



			Entity.Transform.Rotation = rotation;

			targetVelocity = targetVelocity.RotateBy(Transform.Rotation);
			currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity, velocityChangeSpeed * (float)deltaTime);

			position += currentVelocity * (float)deltaTime;

			// make cam on top of the planet
			if (collideWithPlanetSurface)
			{
				var planetLocalPosition = planet.Transform.Position.Towards(position).ToVector3d();
				var sphericalPlanetLocalPosition = planet.CalestialToSpherical(planetLocalPosition);
				var onPlanetSurfaceHeight = planet.GetSurfaceHeight(planetLocalPosition);
				var onPlanetDistanceToSurface = sphericalPlanetLocalPosition.altitude - onPlanetSurfaceHeight;

				var h = onPlanetSurfaceHeight + 2;
				if (sphericalPlanetLocalPosition.altitude <= h || WalkOnPlanet.Bool)
				{
					sphericalPlanetLocalPosition.altitude = h;
					position = planet.Transform.Position + planet.SphericalToCalestial(sphericalPlanetLocalPosition);
				} 
			}


			Entity.Transform.Position = position; // += Entity.Transform.Position.Towards(position).ToVector3d() * deltaTime * 10;

			//Debug.Info(entity.transform.position);


		}

	}
}
