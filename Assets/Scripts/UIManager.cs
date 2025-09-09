using System.Collections.Generic;
using UnityEngine;


public class MenuUI : MonoBehaviour
{
    public void StartGame() => GameManager.Instance.OnStartButton();
    public void ExitGame() => GameManager.Instance.OnQuitButton();
    public void ContinueGame() => GameManager.Instance.OnContinueButton();

    [SerializeField] private GameObject livesPrefab;
    [SerializeField] private RectTransform livesParentElement;

    private List<GameObject> _livesDisplayObjects = new();

    private void Start()
    {
        UpdateLivesDisplay(GameManager.Instance.lives);
    }
    private void UpdateLivesDisplay(int lives)
    {
        // Clear existing lives display
        foreach (var lifeObject in _livesDisplayObjects)
        {
            if (lifeObject != null)
                Destroy(lifeObject);
        }
        _livesDisplayObjects.Clear();

        // Create new lives display
        for (int i = 0; i < lives; i++)
        {
            if (livesPrefab != null && livesParentElement != null)
            {
                var lifeObject = Instantiate(livesPrefab, livesParentElement);
                _livesDisplayObjects.Add(lifeObject);
            }
        }
    }

   
}
