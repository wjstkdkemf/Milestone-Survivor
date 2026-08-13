using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TeleportPreviewMaker : MonoBehaviour
{
    [SerializeField] private GameObject previewRoot;
    [SerializeField] private Image previewImage;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private string emptyDescription = "";

    private void Awake()
    {
        if (previewRoot == null)
            previewRoot = gameObject;

        Clear();
    }

    public void Draw(TeleportData point)
    {
        if (point == null)
        {
            Clear();
            return;
        }

        if (previewRoot != null)
            previewRoot.SetActive(true);

        if (titleText != null)
            titleText.text = point.GetDisplayName();

        if (descriptionText != null)
            descriptionText.text = point.GetDescription();

        if (previewImage != null)
        {
            previewImage.sprite = point.previewImage;
            previewImage.enabled = point.previewImage != null;
            previewImage.preserveAspect = true;
        }
    }

    public void Clear()
    {
        if (titleText != null)
            titleText.text = "";

        if (descriptionText != null)
            descriptionText.text = emptyDescription;

        if (previewImage != null)
        {
            previewImage.sprite = null;
            previewImage.enabled = false;
        }

        if (previewRoot != null)
            previewRoot.SetActive(false);
    }
}
