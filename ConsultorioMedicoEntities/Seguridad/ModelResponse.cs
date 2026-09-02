namespace ConsultorioMedicoEntities.Seguridad
{
    /// <summary>
    /// Envoltura de respuesta no genérica. Toda la API responde HTTP 200 con esta envoltura;
    /// los errores van en IsSuccess=false + Message en español.
    /// </summary>
    public class ModelResponse
    {
        public ModelResponse() { IsSuccess = false; }
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public object Response { get; set; }
    }

    /// <summary>
    /// Envoltura de respuesta genérica.
    /// </summary>
    public class ModelResponse<T>
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public T Response { get; set; }
    }
}
