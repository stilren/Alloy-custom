using Alloy_custom.Business.CustomValidators;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;
using Assert = NUnit.Framework.Assert;

namespace Alloy_custom.tests
{
    class ValidatorHelperTests
    {
        [TestInitialize]
        public void Setup()
        {
        }

        [TestCase("http://www.youtube.com/v/-kpEP4JeEdc", "https://www.youtube-nocookie.com/embed/-kpEP4JeEdc")]
        [TestCase("http://youtu.be/-kpEP4JeEdc", "https://www.youtube-nocookie.com/embed/-kpEP4JeEdc")]
        [TestCase("https://www.youtube.com/watch?v=-kpEP4JeEdc&feature=popular", "https://www.youtube-nocookie.com/embed/-kpEP4JeEdc")]
        public void GetYotubeId_WithCorrectUrls_ReturnsId(string url, string correctUrl)
        {
            var result = ValidatorHelper.GetYoutubeNoCookieUrl(url);
            Assert.AreEqual(correctUrl, result);
        }

        [TestCase("http://www.dynamicdrive.com/forums/showthread.php?77919-What-is-https-www-youtube-nocookie-com")]
        [TestCase("http://stackoverflow.com/questions/3652046/c-sharp-regex-to-get-video-id-from-youtube-and-vimeo-by-url")]
        [TestCase("https://www.youtube.com")]
        [TestCase("")]
        public void GetYotubeId_WithIncorrectUrls_EmptyString(string url)
        {
            var id = ValidatorHelper.GetYoutubeId(url);
            Assert.AreEqual(string.Empty, id);
        }

        [TestCase("http://www.youtube.com/v/-kpEP4JeEdc", "-kpEP4JeEdc")]
        [TestCase("http://youtu.be/-kpEP4JeEdc", "-kpEP4JeEdc")]
        [TestCase("https://www.youtube.com/watch?v=-kpEP4JeEdc&feature=popular", "-kpEP4JeEdc")]
        public void GetYoutubeNoCookieUrl_WithCorrectUrls_ReturnsCorrectUrl(string url, string idResult)
        {
            var id = ValidatorHelper.GetYoutubeId(url);
            Assert.AreEqual(idResult, id);
        }

        [TestCase("http://www.dynamicdrive.com/forums/showthread.php?77919-What-is-https-www-youtube-nocookie-com")]
        [TestCase("http://stackoverflow.com/questions/3652046/c-sharp-regex-to-get-video-id-from-youtube-and-vimeo-by-url")]
        [TestCase("https://www.youtube.com")]
        [TestCase("")]
        public void GetYotubeId_WithIncorrectUrls_ReturnsEmptyString(string url)
        {
            var id = ValidatorHelper.GetYoutubeId(url);
            Assert.AreEqual(string.Empty, id);
        }

        [Test]
        public void ConvertToNoCookieUrl_WithIdInput_ReturnsCorrectUrl()
        {
            var id = "-kpEP4JeEdc";
            string url = ValidatorHelper.ConvertToNoCookieUrl(id);
            Assert.AreEqual("https://www.youtube-nocookie.com/embed/-kpEP4JeEdc", url);
        }

        [Test]
        public void ConvertToNoCookieUrl_WithEmptyInput_ReturnsEmptyString()
        {
            var id = "";
            string url = ValidatorHelper.ConvertToNoCookieUrl(id);
            Assert.AreEqual("", url);
        }
    }
}
