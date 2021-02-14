using System.Collections;
using UnityEngine;
using Managers;
using Strings;

namespace InGameEvents
{
	public class EventElectricalStorm : EventScriptBase
	{
		public override void OnEventStart()
		{
			if (AnnounceEvent)
			{
				var text = "An electrical storm has been detected in your area, please repair potential electronic overloads.";

				CentComm.MakeAnnouncement(ChatTemplates.CentcomAnnounce, text, CentComm.UpdateSound.Alert);
			}

			if (FakeEvent) return;

			base.OnEventStart();
		}

		public override void OnEventStartTimed()
		{
			var stationBounds = MatrixManager.MainStationMatrix.MatrixBounds;
			var boundsMin = stationBounds.Min;
			var boundsMax = stationBounds.Max;

			float width = 0.35f * (boundsMax.x - boundsMin.x);
			float height = 0.35f * (boundsMax.y - boundsMin.y);
			float minX = Random.Range(boundsMin.x, boundsMax.x - width);
			float minY = Random.Range(boundsMin.y, boundsMax.y - height);

			var region = new Bounds();
			region.SetMinMax(new Vector2(minX, minY), new Vector2(minX + width, minY + height));

			foreach (var light in FindObjectsOfType<LightSource>())
			{
				if (region.Contains(light.gameObject.RegisterTile().WorldPositionServer))
				{
					StartCoroutine(BreakLight(light));
				}
			}
		}

		private IEnumerator BreakLight(LightSource light)
		{
			yield return WaitFor.Seconds(Random.Range(0f, 1f));
			var integrity = light.GetComponent<Integrity>();
			integrity.ApplyDamage(light.integrityThreshBar, AttackType.Internal, DamageType.Burn);
		}
	}
}
