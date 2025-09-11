namespace InmobiliariaApp.Repository
{
    /// <summary>
    /// Define el connection string para los repositorios.
    /// Cambiá usuario/clave/nombre DB según tu entorno.
    /// </summary>
    public abstract class BaseModel
    {
        protected string ConnectionString { get; } =
            "Server=localhost;Port=3306;Database=mi_base_datos;Uid=miusuario;Pwd=mipass;SslMode=None;AllowPublicKeyRetrieval=True;AllowUserVariables=True;";
    }
}
