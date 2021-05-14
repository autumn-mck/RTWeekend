using System;
using System.IO;
using System.Numerics;
using Point3 = System.Numerics.Vector3;
using Colour = System.Numerics.Vector3;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RTWeekend
{
	class Program
	{
		private static List<string>[] imgStrs;
		public static ThreadSafeRandom random = new ThreadSafeRandom();
		private static Camera cam;

		private static HittableList RandomScene()
		{
			HittableList world = new HittableList();

			Material groundMat = new Lambertian(new Colour(0.5f, 0.5f, 0.5f));
			world.Add(new Sphere(new Point3(0, -1000, 0), 1000, groundMat));

			for (int a = -11; a < 11; a++)
			{
				for (int b = -11; b < 11; b++)
				{
					double chooseMat = random.NextDouble();
					Point3 centre = new Point3((float)(a + 0.9 * random.NextDouble()), 0.2f, (float)(b + 0.9 * random.NextDouble()));

					if ((centre - new Point3(4, 0.2f, 0)).Length() > 0.9f)
					{
						Material sphereMat;
						float radius = random.NextDouble() / 10f + 0.15f;
						centre.Y = radius;
						if (chooseMat < 0.8f)
						{
							// Diffuse
							Colour albedo = RandVector(0, 1) * RandVector(0, 1);
							sphereMat =new Lambertian(albedo);
							world.Add(new Sphere(centre, radius, sphereMat));
						}
						else if (chooseMat < 0.95)
						{
							// Metal
							Colour albedo = RandVector(0.5f, 1);
							float fuzz = (float)(random.NextDouble() / 2);
							sphereMat = new Metal(albedo, fuzz);
							world.Add(new Sphere(centre, radius, sphereMat));
						}
						else
						{
							// Glass
							sphereMat = new Dielectric(RandFloat(1.3f, 1.7f));
							world.Add(new Sphere(centre, radius, sphereMat));
						}
					}
				}
			}

			Material material1 = new Dielectric(1.5f);
			world.Add(new Sphere(new Point3(0, 1, 0), 1.0f, material1));

			Material material2 = new Lambertian(new Colour(0.4f, 0.2f, 0.1f));
			world.Add(new Sphere(new Point3(-4, 1, 0), 1.0f, material2));

			Material material3 = new Metal(new Colour(0.7f, 0.6f, 0.5f), 0.0f);
			world.Add(new Sphere(new Point3(4, 1, 0), 1.0f, material3));

			return world;
		}
		
		static void Main()
		{
			// Image
			float aspectRatio = 16f / 9f;
			int imgWidth = (int)(1920 / 1.5);
			int imgHeight = (int)(imgWidth / aspectRatio);
			int samplesPerPixel = 64;
			int maxDepth = 50;

			imgStrs = new List<string>[imgHeight + 1];

			// World
			HittableList world = RandomScene();

			// Camera
			Point3 lFrom = new Point3(13, 2, 3);
			Point3 lAt = new Point3(0, 0, 0);
			Point3 vUp = new Point3(0, 1, 0);
			float aperature = 0.1f;
			float distToFocus = 10;// (lFrom - lAt).Length();
			cam = new Camera(lFrom, lAt, vUp, 20f, aspectRatio, aperature, distToFocus);

			int count = 0;

			imgStrs[0] = new List<string> { $"P3\n{imgWidth} {imgHeight}\n255\n" };
			Parallel.For(0, imgHeight, j =>
			{
				count++;
				Console.Write($"\rScanlines remaining: {imgHeight - count} ");
				imgStrs[imgHeight - j] = CalcRow(imgWidth, imgHeight, samplesPerPixel, maxDepth, world, j);
			});


			Console.WriteLine("\nWriting image...");
			using (FileStream fs = new FileStream(Directory.GetCurrentDirectory() + "\\output.ppm", FileMode.Create))
			{
				StreamWriter sr = new StreamWriter(fs);
				foreach (List<string> ls in imgStrs)
				{
					foreach (string s in ls) sr.Write(s);
				}
				sr.Close();
			}
			Console.WriteLine("Done!");
		}

		private static List<string> CalcRow(int imgWidth, int imgHeight, int samplesPerPixel, int maxDepth, HittableList world, int j)
		{
			List<string> res = new List<string>();
			for (int i = 0; i < imgWidth; i++)
			{
				Colour pixelColour = Colour.Zero;
				for (int s = 0; s < samplesPerPixel; s++)
				{
					float u = (float)((i + random.NextDouble()) / (imgWidth - 1));
					float v = (float)((j + random.NextDouble()) / (imgHeight - 1));
					Ray r = cam.GetRay(u, v);
					pixelColour += RayColour(r, world, maxDepth);
				}
				WriteColour(pixelColour, samplesPerPixel, res);
			}
			return res;
		}

		private static Colour RayColour(Ray r, Hittable world, int depth)
		{
			// If we've exceeded the ray bounce limit, no more light is gathered.
			if (depth <= 0)
				return Colour.Zero;

			HitRecord rec = new HitRecord();
			if (world.Hit(r, 0.001f, float.PositiveInfinity, ref rec))
			{
				Ray scattered = new Ray();
				Colour attenuation = new Colour();
				if (rec.Mat.Scatter(ref r, ref rec, ref attenuation, ref scattered))
					return attenuation * RayColour(scattered, world, depth - 1);
				return Colour.Zero;
			}
			Vector3 unitDirection = Vector3.Normalize(r.Direction);
			float t = 0.5f * (unitDirection.Y + 1f);
			return (1f - t) * Colour.One + t * new Colour(0.5f, 0.7f, 1.0f);
		}

		private static void WriteColour(Colour c, int samplesPerPixel, List<string> toList)
		{
			float r = c.X;
			float g = c.Y;
			float b = c.Z;

			// Divide the colour by the number of samples and gamma-correct for gamma=2.0.
			float scale = 1f / samplesPerPixel;
			r = MathF.Sqrt(scale * r);
			g = MathF.Sqrt(scale * g);
			b = MathF.Sqrt(scale * b);

			// Write the translated [0,255] value of each color component.
			toList.Add($"{Math.Clamp(r, 0, 0.9999f) * 256} {Math.Clamp(g, 0, 0.9999f) * 256} {Math.Clamp(b, 0, 0.9999f) * 256}\n");
		}

		private static float RandFloat(float min, float max)
		{
			double d = random.NextDouble();
			return (float)(min + (max - min) * d);
		}

		public static Vector3 RandInUnitSphere()
		{
			while (true)
			{
				Vector3 p = RandVector(-1f, 1f);
				if (p.LengthSquared() >= 1) continue;
				return p;
			}
		}

		public static Vector3 RandUnitVector()
		{
			return Vector3.Normalize(RandInUnitSphere());
		}

		private static Vector3 RandInHemisphere(Vector3 normal)
		{
			Vector3 in_unit_sphere = RandInUnitSphere();
			if (Vector3.Dot(in_unit_sphere, normal) > 0.0) // In the same hemisphere as the normal
				return in_unit_sphere;
			else
				return -in_unit_sphere;
		}

		private static Vector3 RandVector(float min, float max)
		{
			return new Vector3(RandFloat(min, max), RandFloat(min, max), RandFloat(min, max));
		}

		public static Vector3 RandVecInUnitDisc()
		{
			while (true)
			{
				Vector3 p = new Vector3((float)(random.NextDouble() * 2 - 1), (float)(random.NextDouble() * 2 - 1), 0);
				if (p.LengthSquared() >= 1) continue;
				return p;
			}
		}

		public static bool VecNearZero(Vector3 vec)
		{
			float s = 0.00000001f;
			return (MathF.Abs(vec.X) < s) && (MathF.Abs(vec.Y) < s) && (MathF.Abs(vec.Z) < s);
		}

		public static Vector3 VecReflect(Vector3 v, Vector3 n)
		{
			return v - 2 * Vector3.Dot(v, n) * n;
		}

		public static Vector3 VecRefract(Vector3 uv, Vector3 n, float etaiOverEtat)
		{
			float cosTheta = MathF.Min(Vector3.Dot(-uv, n), 1f);
			Vector3 rOutPerpendicular = etaiOverEtat * (uv + cosTheta * n);
			Vector3 rOutParallel = -MathF.Sqrt(MathF.Abs(1f - rOutPerpendicular.LengthSquared())) * n;
			return rOutPerpendicular + rOutParallel;
		}
	}

	public class ThreadSafeRandom
	{
		private static readonly Random _global = new Random();
		[ThreadStatic] private static Random _local;

		public int Next()
		{
			if (_local == null)
			{
				lock (_global)
				{
					if (_local == null)
					{
						int seed = _global.Next();
						_local = new Random(seed);
					}
				}
			}

			return _local.Next();
		}

		public float NextDouble()
		{
			if (_local == null)
			{
				lock (_global)
				{
					if (_local == null)
					{
						int seed = _global.Next();
						_local = new Random(seed);
					}
				}
			}

			return (float)_local.NextDouble();
		}
	}
}
