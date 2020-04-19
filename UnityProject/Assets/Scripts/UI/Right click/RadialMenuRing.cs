using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

public class RadialMenuRingOptions
{
	// The size of the buttons.
	public float Size;

	// Size and padding scalar.
	public float Scalar;

	// The offset of the ring from the center of the menu.
	public float Offset;

	// The padding around each button. Add padding if the button is not circular or to add some extra spacing.
	public float Padding;

	// The starting angle of the ring.
	public float StartingAngle;

	// Should the ring start active.
	public bool StartActive;

	// Whether to show the labels on the buttons.
	public bool ShowLabels;

	// Whether to pack the items tightly around a larger ring.
	public bool Pack;
}

public class RadialMenuRing
{
	private readonly List<RadialMenuItem> menuItems = new List<RadialMenuItem>();
	private readonly RadialButton buttonPrefab;
	private readonly RadialMenuRingOptions options;
	private RadialMenuItem selected;
	public bool IsActive;
	public float Offset;

	public Action SelectedAction => selected.SelectedAction;

	public RadialMenuRing(RadialButton buttonPrefab, RadialMenuRingOptions options)
	{
		this.buttonPrefab = buttonPrefab;
		this.options = options;
		IsActive = options.StartActive;
	}

	public void Initialize(Transform parent, List<RightClickMenuItem> items)
	{
		float paddedSize = (options.Size + options.Padding) * options.Scalar;
		// Determines the y offset to start with. Expands to fit the number of items in the ring.
		Offset = Mathf.Max(items.Count * paddedSize / DMMath.TAU, paddedSize + options.Offset);

		var (startingAngle, angle) = CalculateAngles(Offset, paddedSize, items.Count);

		var initialPosition = parent.position;
		initialPosition.y += Offset;

		for (var i = 0; i < items.Count; i++)
		{
			var itemAngle = Mathf.Repeat(startingAngle + (angle * i), 360);
			// Rotate the position first so that the button itself isn't rotated as well.
			Vector3 itemPosition = initialPosition.RotateAroundZ(parent.position, itemAngle);
			RadialButton button = Object.Instantiate(buttonPrefab, itemPosition, Quaternion.identity, parent);
			button.gameObject.SetActive(IsActive);
			button.SetButton(items[i], ToggleSelected, options.ShowLabels);

			AddMenuItem(parent, items[i], button, itemAngle);
		}
	}

	private (float startingAngle, float angle) CalculateAngles(float offset, float size, int itemCount)
	{
		var startingAngle = options.StartingAngle;
		float angle;
		if (options.Pack)
		{
			// Calculates an angle so that each item takes up the minimal amount of space on the ring instead of
			// evenly spacing it out. The angle will still expand if the offset from the number of items needs it.
			angle = (((size / 2) * DMMath.TAU) / offset / Mathf.PI) * Mathf.Rad2Deg;
			// Adjusts starting angle to show this ring centered around the selected object in the previous ring.
			startingAngle -= angle * (itemCount - 1) / 2;
		}
		else
		{
			angle = (DMMath.TAU / itemCount) * Mathf.Rad2Deg;
		}

		return (startingAngle, angle);
	}

	private void AddMenuItem(Transform parent, RightClickMenuItem item, RadialButton button, float startAngle)
	{
		if (item.SubMenus == null || item.SubMenus.Count <= 0)
		{
			menuItems.Add(new RadialMenuItem(button, null));
			return;
		}

		var subMenuOptions = new RadialMenuRingOptions
		{
			Size = options.Size,
			Scalar = options.Scalar,
			StartActive = false,
			Offset = Offset,
			ShowLabels = true,
			Padding = 20,
			StartingAngle = startAngle,
			Pack = true
		};

		var subMenu = new RadialMenuRing(buttonPrefab, subMenuOptions);
		var menuItem = new RadialMenuItem(button, subMenu);
		menuItem.SubMenu.Initialize(parent, item.SubMenus);
		menuItems.Add(menuItem);
	}

	public void SetActive(bool active)
	{
		if (active == IsActive) return;

		foreach (var menuItem in menuItems)
		{
			menuItem.SetActive(active);
		}

		if (!active) selected = default;
		IsActive = active;
	}

	public void ToggleSelected(RadialButton radialButton)
	{
		if (selected != default)
			selected.ToggleSubMenu();

		var item = menuItems.Find(menuItem => menuItem.Button == radialButton);
		if (item == default) return;
		item.ToggleSubMenu();
		selected = item;
	}
}