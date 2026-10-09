using UnityEngine;

// Cambia el aspecto del agente para que se reconozca fácilmente en la nave.
public class AgenteVisual : MonoBehaviour
{
    public SpriteRenderer cuerpo;
    public SpriteRenderer visor;
    public SpriteRenderer mochila;
    public Color colorCuerpo = Color.cyan;

    private void Awake()
    {
        if (cuerpo == null) cuerpo = GetComponent<SpriteRenderer>();
        if (cuerpo != null) cuerpo.color = colorCuerpo;
    }

    public void MarcarHuyendo(bool huyendo)
    {
        if (cuerpo != null) cuerpo.color = huyendo ? new Color(1f, 0.65f, 0.15f) : colorCuerpo;
    }
}
