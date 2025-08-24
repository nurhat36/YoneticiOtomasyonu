using Serilog;


namespace YoneticiOtomasyonu.Middleware
{
    

    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public RequestLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            Log.Information("Request başladı: {Method} {Path}", context.Request.Method, context.Request.Path);

            await _next(context);

            Log.Information("Response tamamlandı: {StatusCode}", context.Response.StatusCode);
        }
    }

}
