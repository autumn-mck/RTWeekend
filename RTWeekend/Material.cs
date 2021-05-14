using System;
using System.Numerics;
using Colour = System.Numerics.Vector3;
using Point3 = System.Numerics.Vector3;

namespace RTWeekend
{
	public abstract class Material
	{
		public abstract bool Scatter(ref Ray rIn, ref HitRecord rec, ref Colour attenuation, ref Ray scattered);
	}

	public class Lambertian : Material
	{
		Colour Albedo { get; set; }

		public Lambertian(Colour albedo)
		{
			Albedo = albedo;
		}

		public override bool Scatter(ref Ray rIn, ref HitRecord rec, ref Colour attenuation, ref Ray scattered)
		{
			Vector3 scatterDirection = rec.Normal + Program.RandUnitVector();

			if (Program.VecNearZero(scatterDirection)) scatterDirection = rec.Normal;

			scattered = new Ray(rec.P, scatterDirection);
			attenuation = Albedo;
			return true;
		}
	}

	public class Metal : Material
	{
		Colour Albedo { get; set; }
		float Fuzz { get; set; }

		public Metal(Colour albedo, float fuzz)
		{
			Albedo = albedo;
			Fuzz = fuzz;
		}

		public override bool Scatter(ref Ray rIn, ref HitRecord rec, ref Point3 attenuation, ref Ray scattered)
		{
			Vector3 reflected = Program.VecReflect(Vector3.Normalize(rIn.Direction), rec.Normal);
			scattered = new Ray(rec.P, reflected + Fuzz * Program.RandInUnitSphere());
			attenuation = Albedo;
			return (Vector3.Dot(scattered.Direction, rec.Normal) > 0);
		}
	}

	public class Dielectric : Material
	{
		float IR { get; set; }

		public Dielectric(float iR)
		{
			IR = iR;
		}

		public override bool Scatter(ref Ray rIn, ref HitRecord rec, ref Point3 attenuation, ref Ray scattered)
		{
			attenuation = Colour.One;
			float refractionRatio = rec.FrontFace ? (1f / IR) : IR;

			Vector3 unitDirection = Vector3.Normalize(rIn.Direction);
			float cosTheta = MathF.Min(Vector3.Dot(-unitDirection, rec.Normal), 1f);
			float sinTheta = MathF.Sqrt(1f - cosTheta * cosTheta);

			bool cannotRefract = refractionRatio * sinTheta > 1.0;
			Vector3 direction;

			if (cannotRefract || Reflectance(cosTheta, refractionRatio) > Program.random.NextDouble())
				direction = Program.VecReflect(unitDirection, rec.Normal);
			else
				direction = Program.VecRefract(unitDirection, rec.Normal, refractionRatio);

			scattered = new Ray(rec.P, direction);
			return true;
		}

		private static float Reflectance(float cosine, float idx)
		{
			// Use Schlick's approximation for reflectance.
			float r0 = (1 - idx) / (1 + idx);
			r0 *= r0;
			return r0 + (1 - r0) * MathF.Pow((1 - cosine), 5);
		}
	}

	public class Light : Material
	{
		public Light(Point3 albedo, float luminocity)
		{
			Albedo = albedo;
			Luminocity = luminocity;
		}

		public Colour Albedo { get; set; }
		public float Luminocity { get; set; }
		public override bool Scatter(ref Ray rIn, ref HitRecord rec, ref Point3 attenuation, ref Ray scattered)
		{
			Vector3 scatterDirection = rec.Normal + Program.RandUnitVector();

			if (Program.VecNearZero(scatterDirection)) scatterDirection = rec.Normal;

			scattered = new Ray(rec.P, scatterDirection);
			attenuation = Albedo;
			return true;
		}
	}
}
