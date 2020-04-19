using System;
using System.Collections.Generic;
using UnityEngine;

public class PaginatedData<T>
{
	private readonly IList<T> items;
	private readonly int pageLength;
	private int index;

	public PaginatedData(IList<T> items, int pageLength)
	{
		this.items = items;
		this.pageLength = pageLength;
	}

	public int PageCount => (items.Count + pageLength - 1) / pageLength;

	public int Index
	{
		get => index;
		set
		{
			if (index < 0 && index > PageCount) return;
			index = value;
		}
	}

	public bool IsFirstPage => index == 0;

	public bool IsLastPage => index + 1 >= PageCount;

	public int MovePrevious() => IsFirstPage ? index : --index;

	public int MoveNext() => IsLastPage ? index : ++index;

	public List<T> GetPage() => this[index];

	public List<T> this[int page]
	{
		get
		{
			if (page < 0 || page > PageCount) return null;

			Index = page;
			var list = new List<T>(pageLength);
			var startIndex = pageLength * page;
			for (var i = startIndex; i < startIndex + pageLength && i < items.Count; i++)
			{
				list.Add(items[i]);
			}

			return list;
		}
	}
}