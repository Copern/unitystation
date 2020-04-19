using System;
using System.Linq;
using UnityEngine;
using Random = System.Random;

public static class DMMath
{
	public const float TAU = Mathf.PI * 2;

	public static double Round(double n, double x)
	{
		return Math.Round(n / x) * x;
	}

	public static T Max<T>(T[] itemArr)
	{
		return itemArr.Max();
	}

	public static int Clamp(int val, int min, int max)
	{
		return Mathf.Clamp(val, min, max);
	}

	public static bool Prob(double percent)
	{
		Random rand = new Random(Guid.NewGuid().GetHashCode());
		return rand.NextDouble() < percent / 100.0;
	}
}