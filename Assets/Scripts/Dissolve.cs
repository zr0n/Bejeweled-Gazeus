using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Dissolve : MonoBehaviour
{
    public Renderer renderer;
    public float animationTime = 1f;
    public const string dissolveAmountReference = "_DissolveAmount";
    public UnityEvent onDisappear;
    public UnityEvent onAppear;
    
    float _currentDissolve = 1f;
    float _initialDissolve;
    // Start is called before the first frame update
    void Start()
    {
        if (!renderer)
            renderer = GetComponent<Renderer>();

        _initialDissolve = renderer.material.GetFloat(dissolveAmountReference);
        _currentDissolve = _initialDissolve;

    }

    public void Disappear(System.Action callback = null)
    {
        StartCoroutine(AnimateRenderer(1f, callback ));
    }
    public void Appear(System.Action callback = null)
    {
        StartCoroutine(AnimateRenderer(0f, callback));
    }

    public void Reset()
    {
        _currentDissolve = _initialDissolve;
        foreach (var material in renderer.materials)
        {
            material.SetFloat(dissolveAmountReference, _currentDissolve);
        }
    }

    private IEnumerator AnimateRenderer(float newOpacity = 0f, System.Action callback = null)
    {
        float timeCount = 0f;
        float oldOpacity = _currentDissolve;

        while (true)
        {
            timeCount += Time.deltaTime;
            float alpha = timeCount / animationTime;
            _currentDissolve = Mathf.Lerp(oldOpacity, newOpacity, alpha);
            
            foreach(var material in renderer.materials)
            {
                material.SetFloat(dissolveAmountReference, _currentDissolve);
            }

            alpha = Mathf.Clamp(alpha, 0f, 1f);

            if (alpha == 1f)
            {
                if (newOpacity == 0f)
                {
                    onAppear?.Invoke();
                }
                else
                {
                    onDisappear?.Invoke();
                }
                break;
            }

            yield return null;
        }
        callback?.Invoke();
    }
}
