using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class InputState
{
	public Vector2 PointerPosition { get; private set; }
	public Vector2 PointerDelta { get; private set; }
	public bool PointerPressed { get; private set; }
	public bool PointerPressedThisFrame { get; private set; }
	public bool PointerReleasedThisFrame { get; private set; }

	private Vector2 lastPointerPosition;
	private bool isDragging;

	public void Tick()
	{
		UpdatePointer();
	}

	private void UpdatePointer()
	{
		Vector2 pos;
		bool pressed;
		bool pressedThisFrame;
		bool releasedThisFrame;

		if (Touchscreen.current != null)
		{
			TouchControl touch;

			touch = Touchscreen.current.primaryTouch;

			pos = touch.position.ReadValue();
			pressed = touch.press.isPressed;
			pressedThisFrame = touch.press.wasPressedThisFrame;
			releasedThisFrame = touch.press.wasReleasedThisFrame;
		}
		else if (Mouse.current != null)
		{
			pos = Mouse.current.position.ReadValue();
			pressed = Mouse.current.leftButton.isPressed;
			pressedThisFrame = Mouse.current.leftButton.wasPressedThisFrame;
			releasedThisFrame = Mouse.current.leftButton.wasReleasedThisFrame;
		}
		else
		{
			Debug.LogWarning("No Input Device.");
			return;
		}

		this.PointerPosition = pos;
		this.PointerPressed = pressed;
		this.PointerPressedThisFrame = pressedThisFrame;
		this.PointerReleasedThisFrame = releasedThisFrame;
		this.PointerDelta = Vector2.zero;

		if (pressedThisFrame)
		{
			this.isDragging = true;
			this.lastPointerPosition = pos;
		}
		else if (pressed && this.isDragging)
		{
			this.PointerDelta = pos - this.lastPointerPosition;
			this.lastPointerPosition = pos;
		}
		else if (releasedThisFrame)
			this.isDragging = false;
	}
} 