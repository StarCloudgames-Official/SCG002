using TMPro;
using UnityEngine;

public class PSVSampleCell : PooledScrollCell<PSVSampleData>
{
    [SerializeField] private TextMeshProUGUI sampleText;

    public override void BindData(PSVSampleData data, int index)
    {
        base.BindData(data, index);
        if (sampleText != null)
        {
            sampleText.text = data?.name ?? string.Empty;
        }
    }
}
