using Point3 = System.Numerics.Vector3;
using Colour = System.Numerics.Vector3;
using System.Numerics;

namespace RTWeekend
{
	public class Ray
	{
		public Ray() { }
		public Ray(Point3 origin, Vector3 direction, float time = 0f)
		{
			Origin = origin;
			Direction = direction;
			Time = time;
		}
		public Point3 Origin { get; set; }
		public Vector3 Direction { get; set; } 
		public float Time { get; set; }

		public Point3 At(float t)
		{
			return Origin + t * Direction;
		}
	}
}
