using UnityEngine;

// Esta clase controla cómo se mueve el tripulante y qué hace en la nave.
public class Tripulante : MonoBehaviour
{
    // Estos son los estados por los que puede pasar el tripulante.
    public enum Estado
    {
        Caminando,
        HaciendoTarea,
        Huyendo,
        Eliminado
    }

    public Estado estado = Estado.Caminando;

    public float velocidad = 1.6f;
    public float tiempoTarea = 3f;

    // Compruebo si el tripulante sigue vivo.
    public bool EstaVivo => estado != Estado.Eliminado;

    private EstacionTarea objetivo;
    private Traidor peligro;
    private float progresoTarea;
    private Vector2 destino;
    private AgenteVisual visual;
    private float decisionCada = 0.7f;
    private float relojDecision;
    private global::Simulate simulador;

    private void Start()
    {
        // Guardo la referencia de la simulación y registro este tripulante.
        simulador = global::Simulate.Instancia;

        if (simulador != null)
        {
            simulador.Registrar(this);
        }

        // Obtengo el componente que cambia el aspecto cuando huye.
        visual = GetComponent<AgenteVisual>();

        // Busco la primera estación a la que puedo ir.
        ElegirDestino();
    }

    public void Simulate()
    {
        // No hago nada si el tripulante está eliminado o la simulación está detenida.
        if (!EstaVivo || simulador == null || !simulador.EstaActiva)
        {
            return;
        }

        relojDecision += Time.deltaTime;

        // Si vio una eliminación, intenta alejarse del traidor.
        if (estado == Estado.Huyendo)
        {
            if (peligro == null ||
                !peligro.gameObject.activeInHierarchy ||
                Vector2.Distance(transform.position, peligro.transform.position) > 5.5f)
            {
                // Si ya no hay peligro cerca, vuelve a buscar tareas.
                peligro = null;

                if (visual != null)
                {
                    visual.MarcarHuyendo(false);
                }

                estado = Estado.Caminando;
                ElegirDestino();
            }
            else
            {
                // Calculo hacia qué lado debe moverse para alejarse del traidor.
                Vector2 alejamiento =
                    ((Vector2)transform.position -
                    (Vector2)peligro.transform.position).normalized;

                MoverHacia((Vector2)transform.position + alejamiento * 3f);
                return;
            }
        }

        // Si está haciendo una tarea, espero hasta que termine.
        if (estado == Estado.HaciendoTarea)
        {
            if (objetivo == null || objetivo.Completada)
            {
                estado = Estado.Caminando;
                ElegirDestino();
                return;
            }

            progresoTarea += Time.deltaTime;

            if (progresoTarea >= tiempoTarea)
            {
                // Marco la tarea como terminada y busco otra estación.
                objetivo.CompletarTarea();
                objetivo = null;
                progresoTarea = 0f;
                estado = Estado.Caminando;

                ElegirDestino();
            }

            return;
        }

        // Vuelvo a buscar una estación si ya no tengo un destino válido.
        if (objetivo == null ||
            objetivo.Completada ||
            relojDecision >= decisionCada)
        {
            relojDecision = 0f;
            ElegirDestino();
        }

        // Si llegué a la estación, comienzo a realizar la tarea.
        if (objetivo != null &&
            Vector2.Distance(transform.position, objetivo.transform.position) < 0.45f)
        {
            estado = Estado.HaciendoTarea;
            progresoTarea = 0f;
            objetivo.Ocupar();
        }
        else
        {
            // Si todavía no llegué, sigo caminando hacia el destino.
            MoverHacia(destino);
        }
    }

    private void ElegirDestino()
    {
        // Busco las estaciones que existen en la escena.
        // Uso FindObjectsByType para evitar la función obsoleta.
        EstacionTarea[] disponibles =
            FindObjectsByType<EstacionTarea>(FindObjectsSortMode.None);

        EstacionTarea mejor = null;
        float distancia = float.MaxValue;

        // Elijo la estación disponible que esté más cerca.
        foreach (EstacionTarea estacion in disponibles)
        {
            if (estacion == null || estacion.Completada)
            {
                continue;
            }

            float d = Vector2.Distance(
                transform.position,
                estacion.transform.position
            );

            if (d < distancia)
            {
                distancia = d;
                mejor = estacion;
            }
        }

        objetivo = mejor;

        if (objetivo != null)
        {
            destino = objetivo.transform.position;
        }
        else
        {
            // Si no quedan tareas, se mueve a un punto aleatorio de la nave.
            destino = new Vector2(
                Random.Range(-6f, 6f),
                Random.Range(-3f, 3f)
            );
        }
    }

    private void MoverHacia(Vector2 punto)
    {
        // Muevo el personaje poco a poco hacia el punto elegido.
        transform.position = Vector2.MoveTowards(
            transform.position,
            punto,
            velocidad * Time.deltaTime
        );

        LimitarMapa();
    }

    private void LimitarMapa()
    {
        // Evito que el tripulante salga de los límites de la nave.
        Vector3 p = transform.position;

        p.x = Mathf.Clamp(p.x, -8.3f, 8.3f);
        p.y = Mathf.Clamp(p.y, -4.2f, 4.2f);

        transform.position = p;
    }

    public void PresencioEliminacion(Traidor quien)
    {
        // Si estoy vivo y sé quién fue el traidor, comienzo a huir.
        if (!EstaVivo || quien == null)
        {
            return;
        }

        peligro = quien;
        estado = Estado.Huyendo;

        if (visual != null)
        {
            visual.MarcarHuyendo(true);
        }

        // Dejo de buscar tareas mientras escapo.
        objetivo = null;
    }

    public void Eliminar()
    {
        // Evito eliminar al mismo tripulante más de una vez.
        if (!EstaVivo)
        {
            return;
        }

        estado = Estado.Eliminado;

        // Aviso a la simulación que este tripulante ya no está activo.
        if (simulador != null)
        {
            simulador.Quitar(this);
        }

        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        // Quito el registro si el objeto se destruye.
        if (simulador != null)
        {
            simulador.Quitar(this);
        }
    }
}
