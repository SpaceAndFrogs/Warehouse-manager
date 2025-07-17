using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine;

public class HoldableButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public float repeatRate = 0.3f;
    public float timeScaleChangeAmount = 0.25f;

    private bool isHolding = false;
    private Coroutine holdCoroutine;

    public enum Mode { Increase, Decrease, Stop }
    public Mode mode;

    public void OnPointerDown(PointerEventData eventData)
    {
        isHolding = true;

        if (mode == Mode.Stop)
        {
            TimeManager.instance.TogglePause();
        }
        else
        {
            holdCoroutine = StartCoroutine(InvokeRepeatedly());
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isHolding = false;
        if (holdCoroutine != null)
        {
            StopCoroutine(holdCoroutine);
        }
    }

    IEnumerator InvokeRepeatedly()
    {
        while(isHolding)
        {
            switch (mode)
            {
                case Mode.Increase:
                    TimeManager.instance.ChangeTimeScale(timeScaleChangeAmount);
                    break;
                case Mode.Decrease:
                    TimeManager.instance.ChangeTimeScale(-timeScaleChangeAmount);
                    break;
            }

            yield return new WaitForSecondsRealtime(repeatRate);
        }

        holdCoroutine = null;
    }
}
