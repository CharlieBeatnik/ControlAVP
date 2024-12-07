﻿using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ControlAVP
{
    internal static class ControllerExtensions
    {
        //From https://stackoverflow.com/questions/40912375/return-view-as-string-in-net-core
        public static async Task<string> RenderViewAsync<TModel>(this Controller controller, string viewName, TModel model, bool partial = false)
        {
            ArgumentNullException.ThrowIfNull(controller);

            if (string.IsNullOrEmpty(viewName))
            {
                viewName = controller.ControllerContext.ActionDescriptor.ActionName;
            }

            controller.ViewData.Model = model;

            using var writer = new StringWriter();
            IViewEngine viewEngine = controller.HttpContext.RequestServices.GetService(typeof(ICompositeViewEngine)) as ICompositeViewEngine;
            ViewEngineResult viewResult = viewEngine.FindView(controller.ControllerContext, viewName, !partial);

            if (viewResult.Success == false)
            {
                throw new ArgumentException($"A view with the name {viewName} could not be found.");
            }

            ViewContext viewContext = new(
                controller.ControllerContext,
                viewResult.View,
                controller.ViewData,
                controller.TempData,
                writer,
                new HtmlHelperOptions()
            );

            await viewResult.View.RenderAsync(viewContext).ConfigureAwait(false);

            return writer.GetStringBuilder().ToString();
        }
    }
}