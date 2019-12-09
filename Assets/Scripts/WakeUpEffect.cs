using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class WakeUpEffect : MonoBehaviour
{
    private PostProcessVolume volume;
    private Vignette _vignette;
    
    // Start is called before the first frame update
    void Start()
    {
        volume = gameObject.GetComponent<PostProcessVolume>();
        volume.profile.TryGetSettings(out _vignette);
    }

    // Update is called once per frame
    void Update()
    {
        if (_vignette.intensity > 0)
            _vignette.intensity.value = Mathf.Lerp(_vignette.intensity, 0, 1 * Time.deltaTime);
    }
}
