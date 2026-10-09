using System.Collections.Generic;
using UnityEngine;

// Esta clase coordina los pasos de la simulación y actualiza las entidades.
public class Simulate : MonoBehaviour
{
    public static Simulate Instancia { get; private set; }

    private readonly List<Tripulante> tripulantes = new List<Tripulante>();
    private readonly List<Traidor> traidores = new List<Traidor>();
    private readonly List<EstacionTarea> estaciones = new List<EstacionTarea>();

    public bool EstaActiva { get; private set; }
    public bool EstaPausada { get; private set; }
    public float Tiempo { get; private set; }

    private void Awake()
    {
        Instancia = this;
    }

    public void Registrar(Tripulante entidad)
    {
        if (entidad != null && !tripulantes.Contains(entidad)) tripulantes.Add(entidad);
    }

    public void Registrar(Traidor entidad)
    {
        if (entidad != null && !traidores.Contains(entidad)) traidores.Add(entidad);
    }

    public void Registrar(EstacionTarea entidad)
    {
        if (entidad != null && !estaciones.Contains(entidad)) estaciones.Add(entidad);
    }

    public void Quitar(Tripulante entidad) => tripulantes.Remove(entidad);
    public void Quitar(Traidor entidad) => traidores.Remove(entidad);

    public void Iniciar()
    {
        EstaActiva = true;
        EstaPausada = false;
        Time.timeScale = 1f;
    }

    public void AlternarPausa()
    {
        if (!EstaActiva) return;
        EstaPausada = !EstaPausada;
        Time.timeScale = EstaPausada ? 0f : 1f;
    }

    public void Detener(string mensaje)
    {
        EstaActiva = false;
        EstaPausada = false;
        Time.timeScale = 1f;
        if (ControladorEscena.Instancia != null) ControladorEscena.Instancia.MostrarResultado(mensaje);
    }

    private void Update()
    {
        if (!EstaActiva || EstaPausada) return;
        Tiempo += Time.deltaTime;

        // Todas las entidades actualizan su comportamiento desde este coordinador.
        for (int i = tripulantes.Count - 1; i >= 0; i--)
            if (tripulantes[i] != null) tripulantes[i].Simulate();
        for (int i = traidores.Count - 1; i >= 0; i--)
            if (traidores[i] != null) traidores[i].Simulate();
        for (int i = estaciones.Count - 1; i >= 0; i--)
            if (estaciones[i] != null) estaciones[i].Simulate();

        RevisarFinal();
        if (ControladorEscena.Instancia != null) ControladorEscena.Instancia.ActualizarPanel();
    }

    private void RevisarFinal()
    {
        int vivos = 0;
        foreach (var t in tripulantes) if (t != null && t.EstaVivo) vivos++;
        int traidoresVivos = 0;
        foreach (var t in traidores) if (t != null && t.gameObject.activeInHierarchy) traidoresVivos++;

        if (vivos == 0) Detener("Todos los tripulantes fueron eliminados.");
        else if (traidoresVivos == 0) Detener("¡La nave está a salvo!");
        else if (ControladorEscena.Instancia != null &&
                 ControladorEscena.Instancia.TareasCompletadas >= ControladorEscena.Instancia.TotalTareas)
            Detener("¡Todas las tareas están completas!");
    }

    public int TripulantesVivos
    {
        get { int n = 0; foreach (var t in tripulantes) if (t != null && t.EstaVivo) n++; return n; }
    }
    public int TraidoresVivos
    {
        get { int n = 0; foreach (var t in traidores) if (t != null && t.gameObject.activeInHierarchy) n++; return n; }
    }
    public int TotalTripulantes => tripulantes.Count;
}
