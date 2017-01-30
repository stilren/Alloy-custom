using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Alloy_custom.Business.CustomValidators;
using Alloy_custom.Models.Media;
using EPiServer.DataAbstraction;
using EPiServer;
using EPiServer.Core;
using EPiServer.DataAnnotations;
using EPiServer.Shell.ObjectEditing;
using EPiServer.Web;

namespace Alloy_custom.Models.Blocks
{
    /// <summary>
    /// Used to insert a link which is styled as a button
    /// </summary>
    [SiteContentType(GUID = "8dbca1df-a972-4300-a5c0-44b69d65aaba")]
    public class MediaBlock : SiteBlockData
    {
        [CultureSpecific]
        [Display(
           GroupName = SystemTabNames.Content,
           Order = 110)]
        public virtual string YouTubeLink
        {
            get
            {
                var url = this.GetPropertyValue(x => x.YouTubeLink);
                if (!string.IsNullOrEmpty(url))
                {
                    return ValidatorHelper.GetYoutubeNoCookieUrl(url);
                }
                return null;
            }
            set
            {
                this.SetPropertyValue(x => x.YouTubeLink, value);
            }
        }


        [Display(
            GroupName = SystemTabNames.Content,
            Order = 95)]
        [AllowedTypes(typeof(ImageFile))]
        [UIHint(UIHint.Image)]
        public virtual ContentReference TopImage { get; set; }

        [CultureSpecific]
        [Display(Order = 120)]
        [SelectOne(SelectionFactoryType = typeof(WizardTypeSelectionFactory))]
        public virtual Color ButtonColor { get; set; }

    }

    public class WizardTypeSelectionFactory : ISelectionFactory
    {
        public IEnumerable<ISelectItem> GetSelections(ExtendedMetadata metadata)
        {
            return new ISelectItem[]
            {
                new SelectItem()
                {
                    Text = "White",
                    Value = Color.White,
                },
                new SelectItem()
                {
                    Text = "Grey",
                    Value = Color.Grey,
                },
                new SelectItem()
                {
                    Text = "Black",
                    Value = Color.Black,
                },

            };
        }

    }
    public enum Color { Black, White, Grey };
}
