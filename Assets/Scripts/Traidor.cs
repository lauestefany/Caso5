using UnityEngine;

// Esta clase controla cómo se mueve el traidor y cómo busca a sus víctimas.
public class Traidor : MonoBehaviour
{
    // Estos son los estados por los que puede pasar el traidor.
    public enum Estado
    {
        Patrullando,
        BuscandoVictima,
        Atacando,
        Esperando
    }

    public Estado estado = Estado.Patrullando;

    public float velocidad = 1.25f;
    public float distanciaAtaque = 0.65f;
    public float distanciaSeguridad = 2.0f;
    public float enfriamientoAtaque = 5f;

    private float relojAtaque;
    private Vector2 destino;
    private float relojPatrulla;
    private global::Simulate simulador;

    private void Start()
    {
        // Guardo la simulación y registro al traidor para que pueda actualizarse.
        simulador = global::Simulate.Instancia;

        if (simulador != null)
        {
            simulador.Registrar(this);
        }

        // Elijo un punto al azar para comenzar a patrullar.
        ElegirPunto();
    }

    public void Simulate()
    {
        // No hago nada si la simulación está detenida o el traidor está inactivo.
        if (simulador == null ||
            !simulador.EstaActiva ||
            !gameObject.activeInHierarchy)
        {
            return;
        }

        // Actualizo los tiempos del ataque y del recorrido.
        relojAtaque -= Time.deltaTime;
        relojPatrulla += Time.deltaTime;

        // Busco si hay algún tripulante cerca.
        Tripulante victima = BuscarVictima();

        if (victima == null)
        {
            // Si no encuentro a nadie, sigo patrullando por la nave.
            estado = Estado.Patrullando;

            if (relojPatrulla > 2.5f)
            {
                ElegirPunto();
                relojPatrulla = 0f;
            }

            transform.position = Vector2.MoveTowards(
                transform.position,
                destino,
                velocidad * Time.deltaTime
            );

            LimitarMapa();
            return;
        }

        // Si encuentro a alguien, comienzo a acercarme.
        estado = Estado.BuscandoVictima;

        float distancia = Vector2.Distance(
            transform.position,
            victima.transform.position
        );

        if (distancia > distanciaAtaque)
        {
            transform.position = Vector2.MoveTowards(
                transform.position,
                victima.transform.position,
                velocidad * Time.deltaTime
            );

            LimitarMapa();
            return;
        }

        // Espero a que termine el tiempo entre ataques.
        if (relojAtaque > 0f)
        {
            estado = Estado.Esperando;
            return;
        }

        // Solo ataco si no hay otro tripulante cerca de la víctima.
        Tripulante[] tripulantes =
            FindObjectsByType<Tripulante>(FindObjectsSortMode.None);

        foreach (Tripulante otro in tripulantes)
        {
            if (otro == null || otro == victima || !otro.EstaVivo)
            {
                continue;
            }

            if (Vector2.Distance(
                otro.transform.position,
                victima.transform.position
            ) <= distanciaSeguridad)
            {
                // Si alguien puede proteger a la víctima, espero otra oportunidad.
                estado = Estado.Esperando;
                return;
            }
        }

        // Antes de eliminar a la víctima, aviso a los tripulantes cercanos.
        estado = Estado.Atacando;

        foreach (Tripulante testigo in tripulantes)
        {
            if (testigo == null || testigo == victima || !testigo.EstaVivo)
            {
                continue;
            }

            if (Vector2.Distance(
                testigo.transform.position,
                victima.transform.position
            ) <= 4.0f)
            {
                testigo.PresencioEliminacion(this);
            }
        }

        // Elimino a la víctima y activo el tiempo de espera para otro ataque.
        victima.Eliminar();
        relojAtaque = enfriamientoAtaque;
    }

    private Tripulante BuscarVictima()
    {
        // Busco los tripulantes que siguen vivos en la escena.
        Tripulante[] tripulantes =
            FindObjectsByType<Tripulante>(FindObjectsSortMode.None);

        Tripulante mejor = null;
        float menor = float.MaxValue;

        // Elijo al tripulante vivo más cercano dentro del rango de búsqueda.
        foreach (Tripulante tripulante in tripulantes)
        {
            if (tripulante == null || !tripulante.EstaVivo)
            {
                continue;
            }

            float distancia = Vector2.Distance(
                transform.position,
                tripulante.transform.position
            );

            if (distancia < menor && distancia < 5.0f)
            {
                menor = distancia;
                mejor = tripulante;
            }
        }

        return mejor;
    }

    private void ElegirPunto()
    {
        // Elijo un punto aleatorio dentro de los límites de la nave.
        destino = new Vector2(
            Random.Range(-7.5f, 7.5f),
            Random.Range(-3.5f, 3.5f)
        );
    }

    private void LimitarMapa()
    {
        // Evito que el traidor salga del espacio de la nave.
        Vector3 posicion = transform.position;

        posicion.x = Mathf.Clamp(posicion.x, -8.3f, 8.3f);
        posicion.y = Mathf.Clamp(posicion.y, -4.2f, 4.2f);

        transform.position = posicion;
    }

    private void OnDestroy()
    {
        // Quito el registro cuando el traidor se destruye.
        if (simulador != null)
        {
            simulador.Quitar(this);
        }
    }
}
