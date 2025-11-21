namespace sims_web_app.Data.Model
{
    public class ServiceResponse<T>
    {
        public T? Data { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }

        public ServiceResponse(string message, bool success = false, T? data = default(T?))
        {
            Message = message;
            Data = data;
            Success = success;
        }
    }
}
