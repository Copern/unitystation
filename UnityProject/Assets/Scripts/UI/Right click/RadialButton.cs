using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class RadialButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
	private static readonly Color SelectedColor = Color.white / 3f;

	[SerializeField] private Image circle;
	[SerializeField] private Image icon;
	private List<Color> palette;
	[SerializeField] private Text title;
	private Color defaultColour;
	[HideInInspector]
	public Action action;
	private bool isSelected;
	public int size;

	private Action<RadialButton> setSelected;

	private Color Color => isSelected ? defaultColour : defaultColour + SelectedColor;

	private void Awake()
	{
		// unity doesn't support property blocks on ui renderers, so this is a workaround
		icon.material = Instantiate(icon.material);
	}

	public void SetButton(RightClickMenuItem menuItem, Action<RadialButton> toggleSelected, bool showLabel = false)
	{
		setSelected = toggleSelected;
		defaultColour = menuItem.BackgroundColor;
		circle.color = Color;
		action = menuItem.Action;

		size = (int)Mathf.Ceil(circle.preferredWidth);
		icon.sprite = menuItem.IconSprite;
		palette = menuItem.palette;
		if (palette != null)
		{
			icon.material.SetInt("_IsPaletted", 1);
			icon.material.SetColorArray("_ColorPalette", menuItem.palette.ToArray());
		}
		else
		{
			icon.material.SetInt("_IsPaletted", 0);
		}

		if (menuItem.BackgroundSprite != null)
		{
			circle.sprite = menuItem.BackgroundSprite;
		}

		if (showLabel)
		{
			title.text = menuItem.Label;
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		setSelected?.Invoke(this);
		isSelected = true;
		circle.color = Color;
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		isSelected = false;
		circle.color = Color;
	}
}