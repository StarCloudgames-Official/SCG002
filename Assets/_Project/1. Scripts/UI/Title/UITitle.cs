using UnityEngine;

public class UITitle : UIPanel
{
    [SerializeField] private ExtensionSlider gaugeSlider;
    public ExtensionSlider GaugeSlider => gaugeSlider;
}