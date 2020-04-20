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
	private bool isSelected;

	private Action<RadialButton> setSelected;

	private bool IsSelected
	{
		get => isSelected;
		set
		{
			isSelected = value;
			circle.color = Color;
		}
	}

	private Color Color => isSelected ? defaultColour : defaultColour + SelectedColor;

	private void Awake()
	{
		// unity doesn't support property blocks on ui renderers, so this is a workaround
		icon.material = Instantiate(icon.material);
	}

	private void OnEnable()
	{
		IsSelected = false;
	}

	public void SetButton(RightClickMenuItem menuItem, Action<RadialButton> toggleSelected, bool showLabel = false)
	{
		if (menuItem.Action != null)
		{
			var button = gameObject.GetComponent<Button>();
			button.onClick.AddListener(() =>
			{
				menuItem.Action();
				Destroy(gameObject.GetParent());
			});
		}

		setSelected = toggleSelected;
		defaultColour = menuItem.BackgroundColor;
		circle.color = Color;
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
		IsSelected = true;
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		IsSelected = false;
	}
}