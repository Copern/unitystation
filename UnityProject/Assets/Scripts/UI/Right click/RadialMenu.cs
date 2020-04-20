using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;
using Button = UnityEngine.UI.Button;

public class RadialMenu : MonoBehaviour
{
	public RadialButton buttonPrefab;
	public Button pageLeft;
	public Button pageRight;

	[SerializeField]
	[Tooltip("The maximum number of items the main radial ring can have per page.")]
	private int maxItemsPerPage;

	private CameraZoomHandler zoomHandler;
	private PaginatedData<RightClickMenuItem> pages;
	private List<RadialMenuRing> rings;
	private float buttonSize;
	private float sizeScalar;

	public void SetupMenu(List<RightClickMenuItem> menuItems)
	{
		zoomHandler = UIManager.Instance.GetComponent<CameraZoomHandler>();
		zoomHandler.ToggleScrollWheelZoom(false);
		SetupButtonSize();
		pages = new PaginatedData<RightClickMenuItem>(menuItems, maxItemsPerPage);
		rings = new List<RadialMenuRing>(pages.PageCount);
		UpdateMenu();
		SetupPageButton(pageLeft, pages.MovePrevious);
		SetupPageButton(pageRight, pages.MoveNext);
	}

	private void SetupButtonSize()
	{
		var lossyScale = transform.lossyScale;
		sizeScalar = Mathf.Max(lossyScale.x, lossyScale.y);
		var rect = buttonPrefab.GetComponent<RectTransform>().rect;
		buttonSize = Math.Max(rect.width, rect.height);
	}

	private void SetupPageButton(Button button, Func<int> pageAction)
	{
		if (button == null || pageAction == null)
		{
			Logger.LogWarningFormat("Unable to add an action to {0} page button.", Category.UI, gameObject.name);
			return;
		}

		button.onClick.AddListener(() => ChangePage(pageAction));
	}

	private void ChangePage(Func<int> pageAction)
	{
		CurrentRing.SetActive(false);
		pageAction();
		UpdateMenu();
	}

	public void UpdateMenu()
	{
		CurrentRing.SetActive(true);

		pageLeft.SetActive(!pages.IsFirstPage);
		pageRight.SetActive(!pages.IsLastPage);
	}

	/// <summary>
	/// Returns the ring for the current page or creates a new ring for the page if it doesn't exist.
	/// </summary>
	public RadialMenuRing CurrentRing
	{
		get
		{
			var index = pages.Index;
			if (index < rings.Count && rings[index] != null)
			{
				return rings[index];
			}

			var ringItems = pages.GetPage();
			var options = new RadialMenuRingOptions
			{
				StartActive = true,
				Size = buttonSize,
				Scalar = sizeScalar,
				// If there are multiple pages, make sure the offset is the same regardless of how many items there are.
				Offset = pages.IsFirstPage ? 0 : rings[0].Offset - (buttonSize * sizeScalar)
			};
			rings.Add(new RadialMenuRing(buttonPrefab, options));
			rings[index].Initialize(transform, ringItems);
			return rings[index];
		}
	}

	/// <summary>
	/// Gets a new PointerEventData for use with raycasting if the left, middle, or right mouse button was clicked.
	/// </summary>
	/// <returns>A new PointerEventData or null if no button was pressed.</returns>
	private PointerEventData GetPointerDownEvent()
	{
		int? pointerId = null;
		for (var i = 0; i < 3; i++)
		{
			if (CommonInput.GetMouseButtonDown(i))
				pointerId = (i + 1) * -1;
		}

		if (pointerId == null)
			return null;

		return new PointerEventData(EventSystem.current)
		{
			pointerId = pointerId.Value,
			position = CommonInput.mousePosition
		};
	}

	/// <summary>
	/// Checks if any part of this radial menu has been clicked.
	/// </summary>
	/// <param name="pointerEvent"></param>
	/// <returns></returns>
	private bool IsClickedOn(PointerEventData pointerEvent)
	{
		var raycastResults = new List<RaycastResult>();
		EventSystem.current.RaycastAll(pointerEvent, raycastResults);
		foreach (var result in raycastResults)
		{
			var go = result.gameObject;
			while (go != null)
			{
				if (go == gameObject)
					return true;

				go = go.GetParent();
			}
		}

		return false;
	}

	private void Update()
	{
		// Allow mouse wheel to scroll through pages.
		if (Input.mouseScrollDelta.y > 0 && !pages.IsFirstPage)
			ChangePage(pages.MovePrevious);
		if (Input.mouseScrollDelta.y < 0 && !pages.IsLastPage)
			ChangePage(pages.MoveNext);

		var pointerEvent = GetPointerDownEvent();
		if (pointerEvent == null) return;

		// Destroy the menu if there was a mouse click outside of the menu.
		if (!IsClickedOn(pointerEvent))
			Destroy(gameObject);
	}

	private void OnDestroy()
	{
		zoomHandler.OrNull()?.ToggleScrollWheelZoom(true);
	}
}