using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;

[RequireComponent(typeof(Image))]
public class JoystickMachine : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
{
    [SerializeField] RectTransform joystickBackground;
    [SerializeField] RectTransform joystick;

    private Action<Vector2> _dragCallback;

    public Vector2 inputDirection;
    private bool isTouching;

    private void Reset()
    {
        if (joystickBackground == null)
            joystickBackground = GetComponent<RectTransform>();
    }

    private void Update()
    {
        // OnKeyEvent();

        if (isTouching)
        {
            _dragCallback?.Invoke(Vector3.Normalize(inputDirection));
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(joystickBackground, eventData.position, eventData.pressEventCamera, out localPoint);

        inputDirection = localPoint;
        inputDirection = Vector2.ClampMagnitude(inputDirection, joystickBackground.sizeDelta.x * 0.5f);
        joystick.anchoredPosition = inputDirection;

        inputDirection =  inputDirection / (joystickBackground.sizeDelta.x * 0.5f);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnDrag(eventData);
        isTouching = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        inputDirection = Vector2.zero;
        joystick.anchoredPosition = inputDirection;
        isTouching = false;
    }

    public void AddDragEvent(Action<Vector2> inCallback) =>
        _dragCallback = inCallback;

    
    public void OnKeyEvent()
    {
        inputDirection = Vector2.zero;
        isTouching = true;

        if (Input.GetKey(KeyCode.W))
        {
            inputDirection.y = 1;
        }

        if (Input.GetKey(KeyCode.S))
        {
            inputDirection.y = -1;
        }

        if (Input.GetKey(KeyCode.A))
        {
            inputDirection.x = -1;
        }

        if (Input.GetKey(KeyCode.D))
        {
            inputDirection.x = 1;
        }
    }
}