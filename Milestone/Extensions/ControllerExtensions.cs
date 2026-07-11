using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;

namespace Milestone.Extensions;

public static class ControllerExtensions
{
    // Renders a partial view to an HTML string so it can be embedded in a JSON AJAX response.
    public static async Task<string> RenderPartialViewToStringAsync(this Controller controller, string viewName, object model)
    {
        controller.ViewData.Model = model;
        using var writer = new StringWriter();

        var viewEngine = controller.HttpContext.RequestServices.GetRequiredService<ICompositeViewEngine>();
        var viewResult = viewEngine.FindView(controller.ControllerContext, viewName, isMainPage: false);

        if (viewResult.View == null)
            throw new InvalidOperationException($"View '{viewName}' not found");

        var viewContext = new ViewContext(
            controller.ControllerContext,
            viewResult.View,
            controller.ViewData,
            controller.TempData,
            writer,
            new HtmlHelperOptions());

        await viewResult.View.RenderAsync(viewContext);
        return writer.ToString();
    }
}
