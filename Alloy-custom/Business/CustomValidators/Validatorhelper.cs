using System.Linq;
using System.Text.RegularExpressions;
using Castle.Core.Internal;

namespace Alloy_custom.Business.CustomValidators
{
    public class ValidatorHelper
    {
        public static string GetYoutubeNoCookieUrl(string url)
        {
            var id = GetYoutubeId(url);
            return ConvertToNoCookieUrl(id);
        }
        public static string GetYoutubeId(string url)
        {
            var urlParts = Regex.Split(url, "youtu(?:\\.be|be\\.com)/(?:.*v(?:/|=)|(?:.*/)?)([a-zA-Z0-9-_]+)").Where(x => !x.IsNullOrEmpty() && !x.Contains("&")); //http://stackoverflow.com/questions/3652046/c-sharp-regex-to-get-video-id-from-youtube-and-vimeo-by-url
            if (urlParts.Count() != 2)
                return string.Empty;
            return urlParts.LastOrDefault();
        }

        public static string ConvertToNoCookieUrl(string id)
        {
            return id == string.Empty ? "" : $"https://www.youtube-nocookie.com/embed/{id}";
        }
    }
}