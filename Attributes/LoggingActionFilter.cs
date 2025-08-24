using Microsoft.AspNetCore.Mvc.Filters;
using Serilog;
namespace YoneticiOtomasyonu.Attributes { 
public class LoggingActionFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        var actionName = context.ActionDescriptor.DisplayName;
        Log.Information("Action çalışıyor: {ActionName}", actionName);
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        var actionName = context.ActionDescriptor.DisplayName;

        if (context.Exception == null)
            Log.Information("Action tamamlandı: {ActionName}", actionName);
        else
            Log.Error(context.Exception, "Action hata verdi: {ActionName}", actionName);
    }
}
}