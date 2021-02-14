using TileManagement;
using UnityEngine;

namespace Tilemaps.Behaviours.Layers
{
	public class MatrixBounds
	{
		private BoundsInt worldBounds;

		private Vector3Int worldMin;

		private Vector3Int worldMax;

		private BoundsInt bounds;

		private Vector3Int min;

		private Vector3Int max;

		public MetaTileMap MetaMap { get; set; }

		public MatrixMove MatrixMove { get; set; }

		public BoundsInt WorldBounds => worldBounds;

		public Vector3Int WorldMin => worldMin;

		public Vector3Int WorldMax => worldMax;

		public BoundsInt Bounds => bounds;

		public Vector3Int Min => min;

		public Vector3Int Max => max;

		private BoundsInt RotatedBound(Vector3 topRightWorld, Vector3 bottomLeftWorld)
		{
			var min = bottomLeftWorld;
			var max = topRightWorld;
			MaxMinCheck(ref min, ref max, max);
			MaxMinCheck(ref min, ref max, min);

			return new BoundsInt(min.RoundToInt(), (max - min).RoundToInt());
		}

		private void MaxMinCheck(ref Vector3 min, ref Vector3 max, Vector3 ToCompare)
		{
			if (ToCompare.x > max.x)
			{
				max.x = ToCompare.x;
			}
			else if (min.x > ToCompare.x)
			{
				min.x = ToCompare.x;
			}

			if (ToCompare.y > max.y)
			{
				max.y = ToCompare.y;
			}
			else if (min.y > ToCompare.y)
			{
				min.y = ToCompare.y;
			}
		}

		public void UpdateWorldBounds()
		{
			var metaMap = MetaMap;

			if (metaMap == null)
			{
				worldBounds = new BoundsInt();
				worldMin = worldBounds.min;
				worldMax = worldBounds.max;
				return;
			}

			var topRight = metaMap.CellToWorld(min);
			var bottomLeft = metaMap.CellToWorld(max);
			var matrixMove = MatrixMove;

			if (matrixMove == null || matrixMove.inProgressRotation is null)
			{
				worldBounds = new BoundsInt(topRight.RoundToInt(), (bottomLeft - topRight).RoundToInt());
			}
			else
			{
				worldBounds = RotatedBound(topRight, bottomLeft);
			}

			worldMin = worldBounds.min;
			worldMax = worldBounds.max;
		}

		public void UpdateMatrixBounds()
		{
			var metaMap = MetaMap;

			if (metaMap == null) return;

			var minPos = Vector3Int.one * int.MaxValue;
			var maxPos = Vector3Int.one * int.MinValue;

			foreach (var layer in metaMap.LayersValues)
			{
				var layerBounds = layer.Bounds;

				if (layerBounds.x == 0 && layerBounds.y == 0) continue; // No Tiles

				var oldMin = minPos;
				var oldMax = maxPos;
				var layerMin = layerBounds.min;
				var layerMax = layerBounds.max;

				minPos = Vector3Int.Min(layerBounds.min, minPos);
				maxPos = Vector3Int.Max(layerBounds.max, maxPos);

				if (oldMin == minPos && oldMax == maxPos) continue;

				bounds = new BoundsInt(minPos, maxPos - minPos);
				min = layerMin;
				max = layerMax;
			}

			UpdateWorldBounds();
		}
	}
}
