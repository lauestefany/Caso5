using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// Administro los botones, los textos y el reinicio de la simulación.
public class ControladorEscena : MonoBehaviour
{
    public static ControladorEscena Instancia { get; private set; }

    public Text textoEstado;
    public Text textoResultado;
    public Text textoTareas;

    public Button botonIniciar;
    public Button botonPausa;
    public Button botonReiniciar;

    public int TotalTareas { get; private set; }
    public int TareasCompletadas { get; private set; }

    private void Awake()
    {
        // Guardo esta instancia para que los otros scripts puedan usarla.
        Instancia = this;

        // Cuento las estaciones que hay en la escena al comenzar.
        TotalTareas = FindObjectsByType<EstacionTarea>(
            FindObjectsSortMode.None
        ).Length;

        TareasCompletadas = 0;

        // Dejo vacío el mensaje de resultado al iniciar.
        if (textoResultado != null)
        {
            textoResultado.text = "";
        }
    }

    private void Start()
    {
        // Conecto cada botón con la función que le corresponde.
        if (botonIniciar != null)
        {
            botonIniciar.onClick.AddListener(Iniciar);
        }

        if (botonPausa != null)
        {
            botonPausa.onClick.AddListener(Pausar);
        }

        if (botonReiniciar != null)
        {
            botonReiniciar.onClick.AddListener(Reiniciar);
        }

        // Muestro los datos iniciales en pantalla.
        ActualizarPanel();
    }

    public void Iniciar()
    {
        // Le digo a la simulación que comience.
        if (global::Simulate.Instancia != null)
        {
            global::Simulate.Instancia.Iniciar();
        }

        if (textoResultado != null)
        {
            textoResultado.text = "SIMULACIÓN EN CURSO";
        }
    }

    public void Pausar()
    {
        // Pauso o reanudo la simulación cuando se presiona el botón.
        if (global::Simulate.Instancia != null)
        {
            global::Simulate.Instancia.AlternarPausa();
        }

        // Muestro si la simulación está pausada o sigue funcionando.
        if (textoResultado != null && global::Simulate.Instancia != null)
        {
            textoResultado.text = global::Simulate.Instancia.EstaPausada
                ? "SIMULACIÓN EN PAUSA"
                : "SIMULACIÓN EN CURSO";
        }
    }

    public void Reiniciar()
    {
        // Restablezco el tiempo antes de volver a cargar la escena.
        Time.timeScale = 1f;

        // Cargo otra vez la escena guardada para comenzar desde cero.
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void SumarTarea()
    {
        // Aumento el contador sin superar el total de tareas.
        TareasCompletadas = Mathf.Min(
            TareasCompletadas + 1,
            TotalTareas
        );
    }

    public void MostrarResultado(string mensaje)
    {
        // Muestro el mensaje final que recibe la simulación.
        if (textoResultado != null)
        {
            textoResultado.text = mensaje;
        }
    }

    public void ActualizarPanel()
    {
        // Actualizo la cantidad de tripulantes y traidores activos.
        if (textoEstado != null && global::Simulate.Instancia != null)
        {
            textoEstado.text =
                "Tripulantes: " + global::Simulate.Instancia.TripulantesVivos +
                "   |   Traidores: " + global::Simulate.Instancia.TraidoresVivos;
        }

        // Muestro cuántas tareas se han completado.
        if (textoTareas != null)
        {
            textoTareas.text =
                "Tareas: " + TareasCompletadas + " / " + TotalTareas;
        }
    }

    private void OnDestroy()
    {
        // Quito las conexiones de los botones cuando se destruye este objeto.
        if (botonIniciar != null)
        {
            botonIniciar.onClick.RemoveListener(Iniciar);
        }

        if (botonPausa != null)
        {
            botonPausa.onClick.RemoveListener(Pausar);
        }

        if (botonReiniciar != null)
        {
            botonReiniciar.onClick.RemoveListener(Reiniciar);
        }

        // Evito que otros scripts utilicen una instancia que ya no existe.
        if (Instancia == this)
        {
            Instancia = null;
        }
    }
}
