using UnityEngine;

public class CharacterStunVFXController : MonoBehaviour
{
    public GameObject stunVFXRoot;

    public void ShowStun()
    {
        if (stunVFXRoot != null)
        {
            stunVFXRoot.SetActive(true);
        }
    }

    public void HideStun()
    {
        if (stunVFXRoot != null)
        {
            stunVFXRoot.SetActive(false);
        }
    }

    private void Update()
    {
        // 测试用：按 H 显示眩晕，按 J 关闭眩晕
        if (Input.GetKeyDown(KeyCode.H))
        {
            ShowStun();
        }

        if (Input.GetKeyDown(KeyCode.J))
        {
            HideStun();
        }
    }
}