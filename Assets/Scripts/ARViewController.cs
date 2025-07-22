using UnityEngine;
using UnityEngine.UI;

public class ARViewController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button arViewButton;
    [SerializeField] private Button closeARButton;
    [SerializeField] private GameObject main3DViewerPanel;
    [SerializeField] private GameObject arViewPanel;

    [Header("AR Viewer URL")]
    [SerializeField] private string arViewerURL = "ar-view.html";

    private void Start()
    {
        if (arViewButton != null)
            arViewButton.onClick.AddListener(OpenARView);

        if (closeARButton != null)
            closeARButton.onClick.AddListener(CloseARView);

        // Ensure AR view and close button are hidden at start
        if (arViewPanel != null)
            arViewPanel.SetActive(false);
        if (closeARButton != null)
            closeARButton.gameObject.SetActive(false);
    }

    private void OpenARView()
    {
        if (main3DViewerPanel != null)
            main3DViewerPanel.SetActive(false);
        if (arViewPanel != null)
            arViewPanel.SetActive(true);
        if (closeARButton != null)
            closeARButton.gameObject.SetActive(true);

        // Open AR viewer page (replace with your actual AR page)
        Application.OpenURL(arViewerURL);
    }

    private void CloseARView()
    {
        if (arViewPanel != null)
            arViewPanel.SetActive(false);
        if (main3DViewerPanel != null)
            main3DViewerPanel.SetActive(true);
        if (closeARButton != null)
            closeARButton.gameObject.SetActive(false);
    }
}