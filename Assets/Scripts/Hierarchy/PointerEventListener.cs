using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PointerEventListener : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
	public delegate void PointerEvent(PointerEventData eventData);

	public event PointerEvent PointerDown, PointerUp, PointerClick;
	public event PointerEvent BeginDrag, Drag, EndDrag;

	void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
	{
		if (PointerDown != null)
			PointerDown(eventData);
	}

	void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
	{
		if (PointerUp != null)
			PointerUp(eventData);
	}

	void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
	{
		if (PointerClick != null)
			PointerClick(eventData);
	}

	void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
    {
		if (BeginDrag != null)
			BeginDrag(eventData);
	}

	void IDragHandler.OnDrag(PointerEventData eventData)
    {
		if (Drag != null)
			Drag(eventData);
	}

	void IEndDragHandler.OnEndDrag(PointerEventData eventData)
    {
		if (EndDrag != null)
			EndDrag(eventData);
	}
}
