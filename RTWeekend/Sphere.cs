using System;
using Point3 = System.Numerics.Vector3;
using Colour = System.Numerics.Vector3;
using System.Numerics;

namespace RTWeekend
{
	class Sphere : Hittable
	{
		public Point3 Centre { get; set; }
		public float Radius { get; set; }
		public Material Mat { get; set; }

		public Sphere(Point3 centre, float radius, Material mat)
		{
			Centre = centre;
			Radius = radius;
			Mat = mat;
		}

		public override bool Hit(Ray r, float tMin, float tMax, ref HitRecord rec)
		{
			Vector3 oc = r.Origin - Centre;
			float a = r.Direction.LengthSquared();
			float halfB = Vector3.Dot(oc, r.Direction);
			float c = oc.LengthSquared() - Radius * Radius;

			float discriminant = halfB * halfB - a * c;
			if (discriminant < 0) return false;
			float sqrtd = MathF.Sqrt(discriminant);

			// Find the nearest root that lies in the acceptable range.
			float root = (-halfB - sqrtd) / a;
			if (root < tMin || tMax < root)
			{
				root = (-halfB + sqrtd) / a;
				if (root < tMin || tMax < root)
					return false;
			}

			rec.T = root;
			rec.P = r.At(rec.T);
			Vector3 outwardNormal = (rec.P - Centre) / Radius;
			rec.SetFaceNormal(r, outwardNormal);
			rec.Mat = Mat;

			return true;
		}
	}
}
