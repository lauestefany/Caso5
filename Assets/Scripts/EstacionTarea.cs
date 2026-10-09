using UnityEngine;

// Representa una consola de la nave donde se puede completar una tarea.
public class EstacionTarea : MonoBehaviour
{
    public enum Estado { Disponible, Ocupada, Completada }
    public Estado estado = Estado.Disponible;
    public bool Completada => estado == Estado.Completada;
    private SpriteRenderer indicador;
    private bool ocupada;

    private void Start()
    {
        indicador = GetComponent<SpriteRenderer>();
        if (global::Simulate.Instancia != null) global::Simulate.Instancia.Registrar(this);
        ActualizarColor();
    }

    public void Simulate()
    {
        // La estación actualiza su apariencia según su estado actual.
        ActualizarColor();
    }

    public void Ocupar()
    {
        if (Completada) return;
        ocupada = true;
        estado = Estado.Ocupada;
        ActualizarColor();
    }

    public void CompletarTarea()
    {
        if (Completada) return;
        ocupada = false;
        estado = Estado.Completada;
        if (ControladorEscena.Instancia != null) ControladorEscena.Instancia.SumarTarea();
        ActualizarColor();
    }

    private void ActualizarColor()
    {
        if (indicador == null) indicador = GetComponent<SpriteRenderer>();
        if (indicador == null) return;
        if (estado == Estado.Completada) indicador.color = new Color(0.25f, 0.9f, 0.55f);
        else if (estado == Estado.Ocupada || ocupada) indicador.color = new Color(1f, 0.75f, 0.2f);
        else indicador.color = new Color(0.25f, 0.8f, 1f);
    }
}
