using UnityEngine;

// Modo Libre del README: cualquier contrato, sin límite de presupuesto, para experimentar.
// Lo que se vuela en este modo no cuenta para la campaña ni para el archivo.
public static class FreeMode
{
    public static bool On
    {
        get => PlayerPrefs.GetInt("free_mode", 0) == 1;
        set { PlayerPrefs.SetInt("free_mode", value ? 1 : 0); PlayerPrefs.Save(); }
    }
}
