using System;

public struct RadialMenuItem : IEquatable<RadialMenuItem>
{
	public readonly RadialButton Button;
	public readonly RadialMenuRing SubMenu;

	public RadialMenuItem(RadialButton button, RadialMenuRing subMenu)
	{
		Button = button;
		SubMenu = subMenu;
	}

	public Action SelectedAction =>
		SubMenu == null ? Button.OrNull()?.action : SubMenu.SelectedAction;

	public void SetActive(bool active)
	{
		Button.SetActive(active);
		if (!active && SubMenu != null && SubMenu.IsActive)
			ToggleSubMenu();
	}

	public void ToggleSubMenu() =>
		SubMenu?.SetActive(!SubMenu.IsActive);

	public static bool operator ==(RadialMenuItem left, RadialMenuItem right) =>
		left.Equals(right);

	public static bool operator !=(RadialMenuItem left, RadialMenuItem right) =>
		!left.Equals(right);

	public bool Equals(RadialMenuItem menuItem) =>
		menuItem.Button == Button && menuItem.SubMenu == SubMenu;

	public override int GetHashCode()
	{
		var hash = 17;
		hash = hash * 23 + Button.OrNull()?.GetHashCode() ?? 0;
		hash = hash * 23 + SubMenu?.GetHashCode() ?? 0;
		return hash;
	}
}