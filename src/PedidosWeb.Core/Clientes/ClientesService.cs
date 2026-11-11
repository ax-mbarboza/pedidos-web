namespace PedidosWeb.Core.Clientes;

// Servicio heredado: lo escribió un ex integrante del equipo, con apuro.
// Funciona y tiene 4 tests. Nadie se ha atrevido a tocarlo.
public class ClientesService
{
    private readonly List<Cliente> _l;

    public ClientesService() : this(ClientesDeEjemplo.Crear()) { }

    public ClientesService(List<Cliente> l)
    {
        _l = l;
    }

    /// <summary>
    /// Ficha del cliente para la pantalla, como un solo texto con el formato
    /// <c>"nombre | rut | zona | estado"</c>, por ejemplo
    /// <c>"Ferretería El Clavo | 76123456-0 | Zona centro | Activo"</c>.
    /// </summary>
    /// <remarks>
    /// El RUT sale normalizado (sin puntos ni espacios y con la «k» en mayúscula). La zona sale de la región
    /// del cliente («Zona centro», «Zona sur», «Zona norte» o «Sin zona»). El estado es «Activo» o «Inactivo».
    /// Antes se llamaba <c>proc2</c>; solo cambió el nombre, no el comportamiento.
    /// </remarks>
    /// <param name="id">Código del cliente, por ejemplo "C-001".</param>
    /// <returns>La ficha en texto, o <c>""</c> si el cliente no existe.</returns>
    public string ObtenerFichaTexto(string id)
    {
        Cliente? c = null;
        for (int i = 0; i < _l.Count; i++)
        {
            if (_l[i].Id == id) { c = _l[i]; break; }
        }
        if (c == null) return "";
        string r = c.Rut.Replace(".", "").Replace(" ", "").ToUpper();
        string t = "";
        if (c.Region == "RM") t = "Zona centro";
        else if (c.Region == "V") t = "Zona centro";
        else if (c.Region == "VIII") t = "Zona sur";
        else if (c.Region == "II") t = "Zona norte";
        else t = "Sin zona";
        string e = "";
        if (c.Activo == true) e = "Activo"; else e = "Inactivo";
        return c.Nombre + " | " + r + " | " + t + " | " + e;
    }

    // si existe y está activo
    public bool proc3(string id)
    {
        Cliente? c = null;
        for (int i = 0; i < _l.Count; i++)
        {
            if (_l[i].Id == id) { c = _l[i]; break; }
        }
        if (c == null) return false;
        if (c.Activo == true) return true; else return false;
    }
}
