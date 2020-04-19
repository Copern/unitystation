using UnityEngine;

public static class VectorExtensions
{
	public static Vector3 RotateAround(this Vector3 position, Vector3 pivot, Vector3 axis, float angle) =>
		Quaternion.AngleAxis(angle, axis) * (position - pivot) + pivot;

	public static Vector3 RotateAroundZ(this Vector3 position, Vector3 pivot, float angle) =>
		position.RotateAround(pivot, Vector3.forward, angle);
}