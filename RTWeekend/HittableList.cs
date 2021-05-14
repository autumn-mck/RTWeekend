using System;
using System.Collections.Generic;
using System.Text;

namespace RTWeekend
{
	public class HittableList : Hittable
	{
		List<Hittable> hittables = new List<Hittable>();
		public HittableList() { }
		public HittableList(List<Hittable> hittables) { Add(hittables); }

		public void Clear() { hittables.Clear(); }
		public void Add(Hittable h) { hittables.Add(h); }
		public void Add(List<Hittable> hs) { hittables.AddRange(hs); }

		public override bool Hit(Ray r, float tMin, float tMax, ref HitRecord rec)
		{
			HitRecord tempRec = new HitRecord();
			bool hitAnything = false;
			float closest_so_far = tMax;
			foreach (Hittable h in hittables)
			{
				if (h.Hit(r, tMin, closest_so_far, ref tempRec))
				{
					hitAnything = true;
					closest_so_far = tempRec.T;
					rec = tempRec;
				}
			}
			return hitAnything;
		}
	}
}
