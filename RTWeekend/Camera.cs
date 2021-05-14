using Point3 = System.Numerics.Vector3;
using Colour = System.Numerics.Vector3;
using System.Numerics;
using System;

namespace RTWeekend
{
	class Camera
	{
		public Camera(Point3 lookFrom, Point3 lookAt, Vector3 vUp, float vfov, float aspectRatio, float aperature, float focusDist)
		{
			float theta = vfov / 180f * MathF.PI;
			float h = MathF.Tan(theta / 2);
			float viewportHeight = 2f * h;
			float viewportWidth = aspectRatio * viewportHeight;

			w = Vector3.Normalize(lookFrom - lookAt);
			u = Vector3.Normalize(Vector3.Cross(vUp, w));
			v = Vector3.Cross(w, u);

			Origin = lookFrom;
			Horizontal = focusDist * viewportWidth * u;
			Vertical = focusDist * viewportHeight * v;
			LowerLeftCorner = Origin - Horizontal / 2 - Vertical / 2 - w * focusDist;
			lensRadius = aperature / 2;
		}

		public Ray GetRay(float s, float t)
		{
			Vector3 rd = lensRadius * Program.RandVecInUnitDisc();
			Vector3 offset = u * rd.X + v * rd.Y;

			return new Ray(Origin + offset, LowerLeftCorner + s * Horizontal + t * Vertical - Origin - offset);
		}

		private Point3 Origin { get; set; }
		private Point3 LowerLeftCorner { get; set; }
		private Vector3 Horizontal { get; set; }
		private Vector3 Vertical { get; set; }

		private Vector3 w;
		private Vector3 u;
		private Vector3 v;
		private float lensRadius;
	}
}
