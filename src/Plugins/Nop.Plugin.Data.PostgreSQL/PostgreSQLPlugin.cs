using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Infrastructure;
using Nop.Core.Plugins;
using Nop.Data;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Web.Framework.Infrastructure;

namespace Nop.Plugin.Data.PostgreSQL
{
    /// <summary>
    /// PLugin
    /// </summary>
    public class PostgreSQLPlugin : BasePlugin, IDbPlugin
    {

        public PostgreSQLPlugin()
        {
        }


        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        public override string GetConfigurationPageUrl()
        {
            return string.Empty;
            //return _webHelper.GetStoreLocation() + "Admin/WidgetsNivoSlider/Configure";
        }

        public string DbProvider()
        {
            return "~/Plugins/Data.PostgreSQL/Views/_DbProvider.cshtml";
        }

        public string DbConnectionInfo()
        {
            return "~/Plugins/Data.PostgreSQL/Views/_DbConnectionInfo.cshtml";
        }

        

        /// <summary>
        /// Install plugin
        /// </summary>
        public override void Install()
        {
            //pictures
            //var sampleImagesPath = _fileProvider.MapPath("~/Plugins/Widgets.NivoSlider/Content/nivoslider/sample-images/");

            //settings
            //var settings = new NivoSliderSettings
            //{
            //    Picture1Id = _pictureService.InsertPicture(_fileProvider.ReadAllBytes(sampleImagesPath + "banner1.jpg"), MimeTypes.ImagePJpeg, "banner_1").Id,
            //    Text1 = "",
            //    Link1 = _webHelper.GetStoreLocation(false),
            //    Picture2Id = _pictureService.InsertPicture(_fileProvider.ReadAllBytes(sampleImagesPath + "banner2.jpg"), MimeTypes.ImagePJpeg, "banner_2").Id,
            //    Text2 = "",
            //    Link2 = _webHelper.GetStoreLocation(false)
            //    //Picture3Id = _pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "banner3.jpg"), MimeTypes.ImagePJpeg, "banner_3").Id,
            //    //Text3 = "",
            //    //Link3 = _webHelper.GetStoreLocation(false),
            //};
            //_settingService.SaveSetting(settings);


            //_localizationService.AddOrUpdatePluginLocaleResource("Plugins.Widgets.NivoSlider.Picture1", "Picture 1");
            //_localizationService.AddOrUpdatePluginLocaleResource("Plugins.Widgets.NivoSlider.Picture2", "Picture 2");
            //_localizationService.AddOrUpdatePluginLocaleResource("Plugins.Widgets.NivoSlider.Picture3", "Picture 3");
            //_localizationService.AddOrUpdatePluginLocaleResource("Plugins.Widgets.NivoSlider.Picture4", "Picture 4");
            //_localizationService.AddOrUpdatePluginLocaleResource("Plugins.Widgets.NivoSlider.Picture5", "Picture 5");
            //_localizationService.AddOrUpdatePluginLocaleResource("Plugins.Widgets.NivoSlider.Picture", "Picture");
            //_localizationService.AddOrUpdatePluginLocaleResource("Plugins.Widgets.NivoSlider.Picture.Hint", "Upload picture.");
            //_localizationService.AddOrUpdatePluginLocaleResource("Plugins.Widgets.NivoSlider.Text", "Comment");
            //_localizationService.AddOrUpdatePluginLocaleResource("Plugins.Widgets.NivoSlider.Text.Hint", "Enter comment for picture. Leave empty if you don't want to display any text.");
            //_localizationService.AddOrUpdatePluginLocaleResource("Plugins.Widgets.NivoSlider.Link", "URL");
            //_localizationService.AddOrUpdatePluginLocaleResource("Plugins.Widgets.NivoSlider.Link.Hint", "Enter URL. Leave empty if you don't want this picture to be clickable.");
            //_localizationService.AddOrUpdatePluginLocaleResource("Plugins.Widgets.NivoSlider.AltText", "Image alternate text");
            //_localizationService.AddOrUpdatePluginLocaleResource("Plugins.Widgets.NivoSlider.AltText.Hint", "Enter alternate text that will be added to image.");

            base.Install();
        }

        /// <summary>
        /// Uninstall plugin
        /// </summary>
        public override void Uninstall()
        {
            //settings
            //_settingService.DeleteSetting<NivoSliderSettings>();

            //locales
            //_localizationService.DeletePluginLocaleResource("Plugins.Widgets.NivoSlider.Picture1");
            //_localizationService.DeletePluginLocaleResource("Plugins.Widgets.NivoSlider.Picture2");
            //_localizationService.DeletePluginLocaleResource("Plugins.Widgets.NivoSlider.Picture3");
            //_localizationService.DeletePluginLocaleResource("Plugins.Widgets.NivoSlider.Picture4");
            //_localizationService.DeletePluginLocaleResource("Plugins.Widgets.NivoSlider.Picture5");
            //_localizationService.DeletePluginLocaleResource("Plugins.Widgets.NivoSlider.Picture");
            //_localizationService.DeletePluginLocaleResource("Plugins.Widgets.NivoSlider.Picture.Hint");
            //_localizationService.DeletePluginLocaleResource("Plugins.Widgets.NivoSlider.Text");
            //_localizationService.DeletePluginLocaleResource("Plugins.Widgets.NivoSlider.Text.Hint");
            //_localizationService.DeletePluginLocaleResource("Plugins.Widgets.NivoSlider.Link");
            //_localizationService.DeletePluginLocaleResource("Plugins.Widgets.NivoSlider.Link.Hint");
            //_localizationService.DeletePluginLocaleResource("Plugins.Widgets.NivoSlider.AltText");
            //_localizationService.DeletePluginLocaleResource("Plugins.Widgets.NivoSlider.AltText.Hint");

            base.Uninstall();
        }
    }
}