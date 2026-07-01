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
}