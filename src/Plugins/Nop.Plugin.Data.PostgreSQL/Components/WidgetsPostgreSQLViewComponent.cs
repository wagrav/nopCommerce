using System;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Services.Configuration;
using Nop.Services.Media;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Data.PostgreSQL.Components
{
    [ViewComponent(Name = "PostgreSql")]
    public class WidgetsPostgreSQLViewComponent// : NopViewComponent
    {

        public WidgetsPostgreSQLViewComponent()
        {
        }

        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            //var nivoSliderSettings = _settingService.LoadSetting<NivoSliderSettings>(_storeContext.CurrentStore.Id);

            return null;//return View("~/Plugins/Data.PostgreSQL/Views/PublicInfo.cshtml");
        }
    }
}
