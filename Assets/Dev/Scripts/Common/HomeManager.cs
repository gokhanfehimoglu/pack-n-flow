using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PackNFlow
{
    public class HomeManager : MonoBehaviour
    {
        [SerializeField] private TMP_Text levelText;

        public void OnEnable()
        {
            levelText.SetText($"Level {PlayerPrefs.GetInt(GameSaveKeys.LevelIndexKey, 0) + 1}");
        }

        public void OnStart()
        {
            SceneManager.LoadScene("Dev/Scenes/Gameplay");
        }
    }
}