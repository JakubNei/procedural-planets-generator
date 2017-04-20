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


		float cameraSpeedModifier = 10.0f;
		Point lastMousePos;
		int scrollWheelValue;
		Vector3 currentVelocity;

		Vector3 walkOnSphere_lastUp;
		Vector3 walkOnSphere_lastForward;
		bool walkOnSphere_isFirstRun;

		Camera cam => Scene.MainCamera;
		CVar WalkOnPlanet => Debug.GetCVar("game / walk on planet");
		CVar MoveCameraToSurfaceOnStart => Debug.GetCVar("game / move camera to planet surface on start");

		public ProceduralPlanets planets;


		public override void OnAddedToEntity(Entity entity)
		{
			base.OnAddedToEntity(entity);

			EventSystem.On<InputUpdate>(e => Update((float)e.DeltaTime));
			EventSystem.Once<InputUpdate>(e => Start());
		}

		void Start()
		{
			Input.LockCursor = disabledInput;

			WalkOnPlanet.ToogledByKey(Key.G).OnChangedAndNow((cvar) =>
			{
				if (cvar.Bool)
				{
					walkOnSphere_isFirstRun = true;
				}
			});

			// Transform.Rotation = QuaternionUtility.LookRotation(Constants.Vector3Forward, Constants.Vector3Up);


			var planet = planets?.GetClosestPlanet(Transform.Position);
			if (planet != null)
			{
				Transform.LookAt(planet.Transform.Position);
				if (MoveCameraToSurfaceOnStart)
					Transform.Position = new WorldPos((float)-planet.RadiusMin, 0, 0) + planet.Transform.Position;
			}

			Update(0.1f); // spool up
		}

		WorldPos savedPosition1;
		Quaternion savedRotation1;

		WorldPos savedPosition2;
		Quaternion savedRotation2;

		void Update(float deltaTime)
		{
			var rotation = Transform.Rotation;
			var position = Transform.Position;


			if (Debug.GetCVar("game / camera position 1 / save").EatBoolIfTrue())
			{
				savedPosition1 = position;
				savedRotation1 = rotation;
			}
			if (Debug.GetCVar("game / camera position 1 / load").EatBoolIfTrue())
			{
				position = Transform.Position = savedPosition1;
				rotation = Transform.Rotation = savedRotation1;
			}

			if (Debug.GetCVar("game / camera position 2 / save").EatBoolIfTrue())
			{
				savedPosition2 = position;
				savedRotation2 = rotation;
			}
			if (Debug.GetCVar("game / camera position 2 / load").EatBoolIfTrue())
			{
				position = Transform.Position = savedPosition2;
				rotation = Transform.Rotation = savedRotation2;
			}


			var planet = planets?.GetClosestPlanet(position);


			Debug.AddValue("camera / speed modifier", cameraSpeedModifier);
			Debug.AddValue("camera / position", position);

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


			float planetSpeedModifier = 1;

			if (planet != null)
			{
				var planetLocalPosition = planet.Transform.Position.Towards(position).ToVector3d();
				var sphericalPlanetLocalPosition = planet.CalestialToSpherical(planetLocalPosition);
				var onPlanetSurfaceHeight = planet.GetSurfaceHeight(planetLocalPosition);
				var onPlanetDistanceToSurface = sphericalPlanetLocalPosition.altitude - onPlanetSurfaceHeight;

				{
					Debug.AddValue("camera / distance to surface", onPlanetDistanceToSurface);
					{
						var s = MyMath.SmoothStep(1, 30000, (float)onPlanetDistanceToSurface);
						cam.NearClipPlane = 1000 * s + 0.5f;
						cam.FarClipPlane = 5000000 * s + 100000;
					}
				}


				if (speedBasedOnDistanceToPlanet)
				{
					var s = MyMath.Clamp(onPlanetDistanceToSurface, 1, 30000);
					planetSpeedModifier = (1 + (float)s / 5.0f);
				}

			}

			if (Input.LockCursor)
			{

				if (scrollWheelDelta > 0) cameraSpeedModifier *= 1.3f;
				if (scrollWheelDelta < 0) cameraSpeedModifier /= 1.3f;
				cameraSpeedModifier = MyMath.Clamp(cameraSpeedModifier, 1, 100000);
				float currentSpeed = cameraSpeedModifier * planetSpeedModifier;



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


				if (planet != null && Input.GetKeyDown(Key.C))
				{
					rotation = position.Towards(planet.Transform.Position).ToVector3().LookRot();
				}

				if (planet != null && WalkOnPlanet.Bool)
				{

					var up = planet.Center.Towards(position).ToVector3().Normalized();
					var forward = walkOnSphere_lastForward;

					if (walkOnSphere_isFirstRun)
					{
						walkOnSphere_lastUp = up;

						var pointOnPlanet = planet.Center.Towards(position).ToVector3d();
						var s = planet.CalestialToSpherical(pointOnPlanet);
						//s.latitude += 0.1f;
						//var forwardToPole = pointOnPlanet.Towards(planet.SphericalToCalestial(s)).Normalized().ToVector3().Normalized();
						forward = Constants.Vector3Forward.RotateBy(rotation);
					}
					else
					{
						var upDeltaAngle = up.Angle(walkOnSphere_lastUp);
						var upDeltaRot = Quaternion.FromAxisAngle(up.Cross(walkOnSphere_lastUp), upDeltaAngle).Inverted();

						forward = forward.RotateBy(upDeltaRot);
					}


					var left = up.Cross(forward);

					var rotDelta =
						Quaternion.FromAxisAngle(up, -yawDelta) *
						Quaternion.FromAxisAngle(left, pitchDelta);


					forward = forward.RotateBy(rotDelta);

					{
						// clamping up down rotation
						var maxUpDownAngle = 80;
						var minUp = MyMath.ToRadians(90 - maxUpDownAngle);
						var maxDown = MyMath.ToRadians(90 + maxUpDownAngle);
						var angle = forward.Angle(up);
						if (angle < minUp)
							forward = up.RotateBy(Quaternion.FromAxisAngle(left, minUp));
						else if (angle > maxDown)
							forward = up.RotateBy(Quaternion.FromAxisAngle(left, maxDown));
					}


					forward.Normalize();

					rotation = QuaternionUtility.LookRotation(forward, up);

					walkOnSphere_lastForward = forward;
					walkOnSphere_lastUp = up;
					walkOnSphere_isFirstRun = false;

				}
				else
				{
					var rotDelta =
						Quaternion.FromAxisAngle(-Vector3.UnitX, pitchDelta) *
						Quaternion.FromAxisAngle(-Vector3.UnitY, yawDelta) *
						Quaternion.FromAxisAngle(-Vector3.UnitZ, rollDelta);

					rotation = rotation * rotDelta;

					walkOnSphere_isFirstRun = true;
				}



				Transform.Rotation = rotation;

				targetVelocity = targetVelocity.RotateBy(Transform.Rotation);
				currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity, velocityChangeSpeed * (float)deltaTime);

				position += currentVelocity * (float)deltaTime;

				// make cam on top of the planet
				if (planet != null && collideWithPlanetSurface)
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


				Transform.Position = position; // += Entity.Transform.Position.Towards(position).ToVector3d() * deltaTime * 10;

				//Log.Info(entity.transform.position);
			}

		}

	}
}
