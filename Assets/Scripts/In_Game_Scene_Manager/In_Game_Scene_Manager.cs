using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class In_Game_Scene_Manager : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    public void Main_Menu()
    {
        StartCoroutine(Fade_In_Canvas_Group(canvasGroup, 0.5f));
    }
        public IEnumerator Fade_In_Canvas_Group(CanvasGroup cg, float duration)
    {
        cg.gameObject.SetActive(true);
        cg.alpha = 0f;

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(0f, 1f, t / duration);
            yield return null;
        }

        cg.alpha = 1f;
        SceneManager.LoadScene(0);
    }
}
