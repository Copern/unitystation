using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;
using Unitystation.Options;
using Button = UnityEngine.UI.Button;

public class RadialMenu : MonoBehaviour
{
	public RadialButton buttonPrefab;
	public Button pageLeft;
	public Button pageRight;

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
		pages = new PaginatedData<RightClickMenuItem>(menuItems, 12);
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

	void Update()
	{
		// Allow mouse wheel to scroll through pages.
		if (Input.mouseScrollDelta.y > 0 && !pages.IsFirstPage)
			ChangePage(pages.MovePrevious);
		if (Input.mouseScrollDelta.y < 0 && !pages.IsLastPage)
			ChangePage(pages.MoveNext);

		// Determine if any mouse button has been clicked.
		var pointerId = -1;
		for (var i = 0; i < 3; i++)
		{
			if (CommonInput.GetMouseButtonDown(i))
				pointerId = i;
		}

		if (pointerId < 0) return;

		// Raycast to see if we clicked anywhere that is not the radial menu's buttons

		List<RaycastResult> raycastResults = new List<RaycastResult>();

		var pointerData = new PointerEventData(EventSystem.current)
		{
			pointerId = (pointerId + 1) * -1,
			position = CommonInput.mousePosition
		};
		EventSystem.current.RaycastAll(pointerData, raycastResults);
		foreach (var result in raycastResults)
		{
			var go = result.gameObject;
			while (go != null)
			{
				if (go == gameObject) return;

				go = go.GetParent();
			}
		}

		Destroy(gameObject);
	}

	private void OnDestroy()
	{
		zoomHandler.OrNull()?.ToggleScrollWheelZoom(true);
	}
}