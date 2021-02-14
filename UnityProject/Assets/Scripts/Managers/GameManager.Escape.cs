using System;
using System.Collections;
using UnityEngine;
using Objects.Wallmounts;

/// <summary>
/// Escape-related part of GameManager
/// </summary>
public partial class GameManager
{
	public EscapeShuttle PrimaryEscapeShuttle => primaryEscapeShuttle;
	[SerializeField]
	private EscapeShuttle primaryEscapeShuttle;

	private Coroutine departCoroutine;
	private bool shuttleSent;

	public bool ShuttleSent => shuttleSent;

	private void InitEscapeShuttle ()
	{
		//Primary escape shuttle lookup
		if (!PrimaryEscapeShuttle)
		{
			var shuttles = FindObjectsOfType<EscapeShuttle>();
			if (shuttles.Length != 1)
			{
				Logger.LogError("Primary escape shuttle is missing from GameManager!", Category.Round);
				return;
			}
			Logger.LogWarning("Primary escape shuttle is missing from GameManager, but one was found on scene");
			primaryEscapeShuttle = shuttles[0];
		}
	}

	/// <summary>
	/// Called after MatrixManager is initialized
	/// </summary>
	///
	private void InitEscapeStuff()
	{
		//Primary escape shuttle lookup
		if (PrimaryEscapeShuttle == null)
		{
			var shuttles = FindObjectsOfType<EscapeShuttle>();
			if (shuttles.Length < 1)
			{
				Logger.LogError("Primary escape shuttle is missing from GameManager!", Category.Round);
				return;
			}
			Logger.LogWarning("Primary escape shuttle is missing from GameManager, but one was found on scene");
			primaryEscapeShuttle = shuttles[0];
		}

		//later, maybe: keep a list of all computers and call the shuttle automatically with a 25 min timer if they are deleted

		var matrixInfo = primaryEscapeShuttle.MatrixInfo;
		//Starting up at Centcom coordinates
		if (GameManager.Instance.QuickLoad)
		{
			if (matrixInfo is null) return;
			if (matrixInfo.MatrixMove == null) return;
		}

		var matrixMove = matrixInfo.MatrixMove;

		var orientation = matrixMove.InitialFacing;
		float width;

		var shuttleSize = matrixInfo.MatrixBounds.Bounds.size;

		if (orientation == Orientation.Up || orientation == Orientation.Down)
		{
			width = shuttleSize.x;
		}
		else
		{
			width = shuttleSize.y;
		}

		Vector3 newPos;
		var landingManager = LandingZoneManager.Instance;
		var centcomDockingPos = landingManager.centcomDockingPos;

		switch (landingManager.centcomDocking.orientation)
		{
			case OrientationEnum.Right:
				newPos = new Vector3(centcomDockingPos.x + Mathf.Ceil(width/2f), centcomDockingPos.y, 0);
				break;
			case OrientationEnum.Up:
				newPos = new Vector3(centcomDockingPos.x , centcomDockingPos.y + Mathf.Ceil(width/2f), 0);
				break;
			case OrientationEnum.Left:
				newPos = new Vector3(centcomDockingPos.x - Mathf.Ceil(width/2f), centcomDockingPos.y, 0);
				break;
			default:
				newPos = new Vector3(centcomDockingPos.x , centcomDockingPos.y - Mathf.Ceil(width/2f), 0);
				break;
		}

		matrixMove.ChangeFacingDirection(Orientation.FromEnum(primaryEscapeShuttle.orientationForDockingAtCentcom));
		matrixMove.SetPosition(newPos);
		primaryEscapeShuttle.InitDestination(newPos);

		bool beenToStation = false;

		primaryEscapeShuttle.OnShuttleUpdate?.AddListener( status =>
		{
			//status display ETA tracking
			if ( status == EscapeShuttleStatus.OnRouteStation )
			{
				primaryEscapeShuttle.OnTimerUpdate.AddListener( TrackETA );
			} else
			{
				primaryEscapeShuttle.OnTimerUpdate.RemoveListener( TrackETA );
				CentComm.UpdateStatusDisplay( StatusDisplayChannel.EscapeShuttle, string.Empty);
			}

			if ( status == EscapeShuttleStatus.DockedCentcom && beenToStation )
			{
				Logger.Log("Shuttle arrived at Centcom", Category.Round);
				Chat.AddSystemMsgToChat($"<color=white>Escape shuttle has docked at Centcomm! Round will restart in {TimeSpan.FromSeconds(RoundEndTime).Minutes} minute.</color>", MatrixManager.MainStationMatrix);
				StartCoroutine(WaitForRoundEnd());
			}

			IEnumerator WaitForRoundEnd()
			{
				Logger.Log($"Shuttle docked to Centcom, Round will end in {TimeSpan.FromSeconds(RoundEndTime).Minutes} minute", Category.Round);
				yield return WaitFor.Seconds(1f);
				EndRound();
			}

			if (status == EscapeShuttleStatus.DockedStation && !primaryEscapeShuttle.hostileEnvironment)
			{
				beenToStation = true;
				SoundManager.PlayNetworked(SingletonSOSounds.Instance.ShuttleDocked);
				Chat.AddSystemMsgToChat($"<color=white>Escape shuttle has arrived! Crew has {TimeSpan.FromSeconds(ShuttleDepartTime).Minutes} minutes to get on it.</color>", MatrixManager.MainStationMatrix);
				//should be changed to manual send later
				departCoroutine = StartCoroutine( SendEscapeShuttle( ShuttleDepartTime ) );
			}
			else if (status == EscapeShuttleStatus.DockedStation && primaryEscapeShuttle.hostileEnvironment)
			{
				beenToStation = true;
				SoundManager.PlayNetworked(SingletonSOSounds.Instance.ShuttleDocked);
				Chat.AddSystemMsgToChat($"<color=white>Escape shuttle has arrived! The shuttle <color=#FF151F>cannot</color> leave the station due to the hostile environment!</color>", MatrixManager.MainStationMatrix);
			}
		} );
	}

	public void ForceSendEscapeShuttleFromStation(int departTime)
	{
		if(shuttleSent || PrimaryEscapeShuttle.Status != EscapeShuttleStatus.DockedStation) return;

		if (departCoroutine != null)
		{
			StopCoroutine(departCoroutine);
		}

		departCoroutine = StartCoroutine( SendEscapeShuttle(departTime));
	}

	private void TrackETA(int eta)
	{
		CentComm.UpdateStatusDisplay( StatusDisplayChannel.EscapeShuttle, FormatTime( eta, "STATION\nETA: " ) );
	}

	public static string FormatTime( int timerSeconds, string prefix = "ETA: " )
	{
		if ( timerSeconds < 0 )
		{
			return string.Empty;
		}

		return prefix+TimeSpan.FromSeconds( timerSeconds ).ToString( "mm\\:ss" );
	}

	private IEnumerator SendEscapeShuttle( int seconds )
	{
		//departure countdown
		for ( int i = seconds; i >= 0; i-- )
		{
			CentComm.UpdateStatusDisplay( StatusDisplayChannel.EscapeShuttle, FormatTime(i, "Depart\nETA: ") );
			yield return WaitFor.Seconds(1);
		}

		shuttleSent = true;
		PrimaryEscapeShuttle.SendShuttle();

		//centcom round end countdown
		int timeToCentcom = (ShuttleDepartTime * 2 - 2);
		for ( int i = timeToCentcom - 1; i >= 0; i-- )
		{
			CentComm.UpdateStatusDisplay( StatusDisplayChannel.EscapeShuttle, FormatTime(i, "CENTCOM\nETA: ") );
			yield return WaitFor.Seconds(1);
		}

		CentComm.UpdateStatusDisplay( StatusDisplayChannel.EscapeShuttle, string.Empty);
	}

	private IEnumerator WaitToInitEscape()
	{
		while ( !MatrixManager.IsInitialized )
		{
			yield return WaitFor.EndOfFrame;
		}
		InitEscapeStuff();
	}
}
