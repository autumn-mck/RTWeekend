using Point3 = System.Numerics.Vector3;
using Colour = System.Numerics.Vector3;
using System.Numerics;

namespace RTWeekend
{
	public struct HitRecord
	{
		public Point3 P { get; set; }
		public Vector3 Normal { get; set; }
		public float T { get; set; }
		public Material Mat { get; set; }

		public bool FrontFace { get; set; }

		public void SetFaceNormal(Ray r, Vector3 outwardNormal)
		{
			FrontFace = Vector3.Dot(r.Direction, outwardNormal) < 0;
			Normal = FrontFace? outwardNormal :-outwardNormal;
		}
};

	public abstract class Hittable
	{
		public abstract bool Hit(Ray r, float t_min, float t_max, ref HitRecord rec);
	}
}
