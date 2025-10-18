using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SafetyUIManager : MonoBehaviour
{
    [Header("UI References")]
    public Canvas uiCanvas;
    public GameObject warningPanel;
    public Text warningText;
    public GameObject violationPanel;
    public Text violationText;
    public GameObject scorePanel;
    public Text scoreText;
    
    [Header("Warning Settings")]
    public float warningDisplayTime = 3f;
    public float violationDisplayTime = 5f;
    
    private bool showingWarning = false;
    private bool showingViolation = false;
    
    void Start()
    {
        SetupUI();
    }
    
    void SetupUI()
    {
        // Find or create canvas
        if (uiCanvas == null)
        {
            uiCanvas = FindObjectOfType<Canvas>();
            if (uiCanvas == null)
            {
                CreateCanvas();
            }
        }
        
        CreateWarningPanel();
        CreateViolationPanel();
        CreateScorePanel();
    }
    
    void CreateCanvas()
    {
        GameObject canvasObj = new GameObject("Safety UI Canvas");
        uiCanvas = canvasObj.AddComponent<Canvas>();
        uiCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        uiCanvas.sortingOrder = 10;
        
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();
        
        Debug.Log("Created Safety UI Canvas");
    }
    
    void CreateWarningPanel()
    {
        // Create warning panel
        GameObject warningObj = new GameObject("Warning Panel");
        warningObj.transform.SetParent(uiCanvas.transform, false);
        
        warningPanel = warningObj;
        Image warningBg = warningObj.AddComponent<Image>();
        warningBg.color = new Color(1f, 1f, 0f, 0.8f); // Yellow background
        
        RectTransform warningRect = warningObj.GetComponent<RectTransform>();
        warningRect.anchorMin = new Vector2(0.1f, 0.8f);
        warningRect.anchorMax = new Vector2(0.9f, 0.95f);
        warningRect.sizeDelta = Vector2.zero;
        warningRect.anchoredPosition = Vector2.zero;
        
        // Create warning text
        GameObject textObj = new GameObject("Warning Text");
        textObj.transform.SetParent(warningObj.transform, false);
        
        warningText = textObj.AddComponent<Text>();
        warningText.text = "WARNING MESSAGE";
        warningText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        warningText.fontSize = 24;
        warningText.color = Color.black;
        warningText.alignment = TextAnchor.MiddleCenter;
        warningText.fontStyle = FontStyle.Bold;
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;
        
        warningPanel.SetActive(false);
    }
    
    void CreateViolationPanel()
    {
        // Create violation panel
        GameObject violationObj = new GameObject("Violation Panel");
        violationObj.transform.SetParent(uiCanvas.transform, false);
        
        violationPanel = violationObj;
        Image violationBg = violationObj.AddComponent<Image>();
        violationBg.color = new Color(1f, 0f, 0f, 0.9f); // Red background
        
        RectTransform violationRect = violationObj.GetComponent<RectTransform>();
        violationRect.anchorMin = new Vector2(0.1f, 0.6f);
        violationRect.anchorMax = new Vector2(0.9f, 0.75f);
        violationRect.sizeDelta = Vector2.zero;
        violationRect.anchoredPosition = Vector2.zero;
        
        // Create violation text
        GameObject textObj = new GameObject("Violation Text");
        textObj.transform.SetParent(violationObj.transform, false);
        
        violationText = textObj.AddComponent<Text>();
        violationText.text = "VIOLATION MESSAGE";
        violationText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        violationText.fontSize = 28;
        violationText.color = Color.white;
        violationText.alignment = TextAnchor.MiddleCenter;
        violationText.fontStyle = FontStyle.Bold;
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;
        
        violationPanel.SetActive(false);
    }
    
    void CreateScorePanel()
    {
        // Create score panel
        GameObject scoreObj = new GameObject("Score Panel");
        scoreObj.transform.SetParent(uiCanvas.transform, false);
        
        scorePanel = scoreObj;
        Image scoreBg = scoreObj.AddComponent<Image>();
        scoreBg.color = new Color(0f, 0f, 0f, 0.7f); // Black background
        
        RectTransform scoreRect = scoreObj.GetComponent<RectTransform>();
        scoreRect.anchorMin = new Vector2(0.02f, 0.02f);
        scoreRect.anchorMax = new Vector2(0.3f, 0.2f);
        scoreRect.sizeDelta = Vector2.zero;
        scoreRect.anchoredPosition = Vector2.zero;
        
        // Create score text
        GameObject textObj = new GameObject("Score Text");
        textObj.transform.SetParent(scoreObj.transform, false);
        
        scoreText = textObj.AddComponent<Text>();
        scoreText.text = "Safety Score: 100%\nDistance: 10.5m";
        scoreText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        scoreText.fontSize = 14;
        scoreText.color = Color.white;
        scoreText.alignment = TextAnchor.UpperLeft;
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;
        textRect.offsetMin = new Vector2(10, 10);
        textRect.offsetMax = new Vector2(-10, -10);
        
        scorePanel.SetActive(true);
    }
    
    public void ShowWarning(string message)
    {
        if (showingWarning) return;
        
        StartCoroutine(DisplayWarning(message));
    }
    
    public void ShowViolation(string message)
    {
        if (showingViolation) return;
        
        StartCoroutine(DisplayViolation(message));
    }
    
    public void UpdateScore(float safetyScore, float distanceToHazard, int violations)
    {
        if (scoreText != null)
        {
            scoreText.text = $"Safety Score: {(safetyScore * 100):F0}%\n" +
                           $"Distance: {distanceToHazard:F1}m\n" +
                           $"Violations: {violations}";
        }
    }
    
    IEnumerator DisplayWarning(string message)
    {
        showingWarning = true;
        
        warningText.text = message;
        warningPanel.SetActive(true);
        
        // Fade in
        yield return StartCoroutine(FadePanel(warningPanel, 0f, 1f, 0.3f));
        
        // Wait
        yield return new WaitForSeconds(warningDisplayTime);
        
        // Fade out
        yield return StartCoroutine(FadePanel(warningPanel, 1f, 0f, 0.3f));
        
        warningPanel.SetActive(false);
        showingWarning = false;
    }
    
    IEnumerator DisplayViolation(string message)
    {
        showingViolation = true;
        
        violationText.text = message;
        violationPanel.SetActive(true);
        
        // Flash effect
        for (int i = 0; i < 3; i++)
        {
            yield return StartCoroutine(FadePanel(violationPanel, 0f, 1f, 0.1f));
            yield return StartCoroutine(FadePanel(violationPanel, 1f, 0f, 0.1f));
        }
        
        // Show final message
        yield return StartCoroutine(FadePanel(violationPanel, 0f, 1f, 0.2f));
        yield return new WaitForSeconds(violationDisplayTime);
        yield return StartCoroutine(FadePanel(violationPanel, 1f, 0f, 0.3f));
        
        violationPanel.SetActive(false);
        showingViolation = false;
    }
    
    IEnumerator FadePanel(GameObject panel, float startAlpha, float endAlpha, float duration)
    {
        Image panelImage = panel.GetComponent<Image>();
        if (panelImage == null) yield break;
        
        float elapsed = 0f;
        Color startColor = panelImage.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, endAlpha);
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            float currentAlpha = Mathf.Lerp(startAlpha, endAlpha, t);
            panelImage.color = new Color(startColor.r, startColor.g, startColor.b, currentAlpha);
            
            yield return null;
        }
        
        panelImage.color = endColor;
    }
}