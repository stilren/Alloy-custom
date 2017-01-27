using System;
using System.Collections.Generic;
using System.Linq;
using Alloy.EpiFormsCustom.Actors;
using Castle.Core.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace Alloy_custom.tests
{
    [TestClass]
    public class SendEmailWithAttachmentsAfterSubmissionActorTests
    {
        [TestInitialize]
        public void Setup()
        {
        }

        [TestCase("636154202431246123_", "funnycat", "jpg")]
        [TestCase("636154202431246123_", "funnycat", "")]
        [TestCase("", "funnycat", "jpg")]
        [TestCase("636154202431246123_", "_funnycat", "jpg")]
        public void GetFileNameAndExtension_ShouldGetFilenameAndExtenstion(string episerverAddedName, string filename,
            string extension)
        {
            var dot = extension.IsNullOrEmpty() ? string.Empty : ".";
            var testUrl = "http://test.com/" + episerverAddedName + filename + dot + extension;
            var result = SendEmailWithAttachmentsAfterSubmissionActor.GetFileName(testUrl);
            Assert.AreEqual(filename + dot + extension, result);
        }

        [TestMethod]
        public void GetUploadElements_WithEmptyInput_ShouldReturnEmptyNumerable()
        {
            var result = SendEmailWithAttachmentsAfterSubmissionActor.GetUploadElements(
                new Dictionary<string, object>(), new List<string>());
            Assert.AreEqual(false, result.Any());
        }

        [TestMethod]
        public void GetUploadElements_WithNoMatch_ShouldReturnEmptyNumerable()
        {
            var keys = new List<string>()
            {
                "hej",
                "hå",
                "123"
            };

            var submissionDataDict = new Dictionary<string, object>()
            {
                {"test", "test"},
            };

            var result = SendEmailWithAttachmentsAfterSubmissionActor.GetUploadElements(submissionDataDict, keys);
            Assert.AreEqual(false, result.Any());
        }

        [TestMethod]
        public void GetUploadElements_WithEmptyMatch_ShouldReturnEmptyResult()
        {
            var keys = new List<string>()
            {
                "__field_1",
            };

            var submissionDataDict = new Dictionary<string, object>()
            {
                {"__field_1", ""},
            };

            var result = SendEmailWithAttachmentsAfterSubmissionActor.GetUploadElements(submissionDataDict, keys).ToList();
            Assert.AreEqual(false, result.Any());
        }

        [TestMethod]
        public void GetUploadElements_WithSingleMatch_ShouldReturnSingleResult()
        {
            var keys = new List<string>()
            {
                "__field_1",
                "__field_2",
                "__field_3"
            };

            var submissionDataDict = new Dictionary<string, object>()
            {
                {"__field_1", "result_1"},
                {"__field_11", "result_11" }
            };

            var result = SendEmailWithAttachmentsAfterSubmissionActor.GetUploadElements(submissionDataDict, keys).ToList();
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual("result_1", result.First());
        }

        [TestMethod]
        public void GetUploadElements_WithMultipleMatch_ShouldReturnMultipleResult()
        {

            var keys = new List<string>()
            {
                "__field_1",
                "__field_2",
                "__field_3"
            };

            var submissionDataDict = new Dictionary<string, object>()
            {
                {"__field_1", "result_1"},
                {"__field_2", "result_2"},
                {"__field_11", "result_11" }
            };

            var result = SendEmailWithAttachmentsAfterSubmissionActor.GetUploadElements(submissionDataDict, keys).OrderByDescending(x => x).ToList();
            Assert.AreEqual(2, result.Count());
            Assert.AreEqual("result_2", result[0]);
            Assert.AreEqual("result_1", result[1]);
        }

        [TestMethod]
        public void ParseUrls_WithSingleRowTwoFilesInput_ShouldReturnTwoDownloadFiles()
        {
            var input = new List<string>()
            {
                "/contentassets/819f6cc238fd416cbb58014465476c46/636177619211870734_powerpoint.pptx#@My Powerpoint.pptx|/contentassets/819f6cc238fd416cbb58014465476c46/636177619211870734_another-powerpoint.pptx#@En sprint för PO.pptx"
            };

            var expectedResults = new List<SendEmailWithAttachmentsAfterSubmissionActor.DownloadFile>()
            {
                new SendEmailWithAttachmentsAfterSubmissionActor.DownloadFile()
                {
                    Name = "En sprint för PO.pptx",
                    Url = "/contentassets/819f6cc238fd416cbb58014465476c46/636177619211870734_another-powerpoint.pptx"
                },
                new SendEmailWithAttachmentsAfterSubmissionActor.DownloadFile()
                {
                    Name = "My Powerpoint.pptx",
                    Url = "/contentassets/819f6cc238fd416cbb58014465476c46/636177619211870734_powerpoint.pptx"
                }
            };

            var output = SendEmailWithAttachmentsAfterSubmissionActor.ParseUrls(input);

            var result = output.Count(downloadFile => expectedResults.Exists(x => x.Name == downloadFile.Name && x.Url == downloadFile.Url));
            Assert.AreEqual(2, result);
        }

        [TestMethod]
        public void ParseUrls_WithSingleRowOneFilesInput_ShouldReturnOneDownloadFile()
        {
            var input = new List<string>()
            {
                "/contentassets/819f6cc238fd416cbb58014465476c46/636177619211870734_powerpoint.pptx#@My Powerpoint.pptx"
            };

            var expectedResults = new List<SendEmailWithAttachmentsAfterSubmissionActor.DownloadFile>()
            {
                new SendEmailWithAttachmentsAfterSubmissionActor.DownloadFile()
                {
                    Name = "My Powerpoint.pptx",
                    Url = "/contentassets/819f6cc238fd416cbb58014465476c46/636177619211870734_powerpoint.pptx"
                }
            };

            var output = SendEmailWithAttachmentsAfterSubmissionActor.ParseUrls(input);

            var result = output.Count(downloadFile => expectedResults.Exists(x => x.Name == downloadFile.Name && x.Url == downloadFile.Url));
            Assert.AreEqual(1, result);
        }

        [TestMethod]
        public void ParseUrls_WithDoubleRowTwoFilesInput_ShouldReturnFourDownloadFile()
        {
            var input = new List<string>()
            {
                "/contentassets/819f6cc238fd416cbb58014465476c46/636177619211870734_powerpoint.pptx#@My Powerpoint.pptx|/contentassets/819f6cc238fd416cbb58014465476c46/636177619211870734_another-powerpoint.pptx#@En sprint för PO.pptx|/contentassets/819f6cc238fd416cbb58014465476c46/636177619211870734_another-powerpoint3.pptx#@En sprint för PO3.pptx",
                "/contentassets/819f6cc238fd416cbb58014465476c46/636177619211870734_a-third-powerpoint.pptx#@A third powerpoint.pptx|/contentassets/819f6cc238fd416cbb58014465476c46/636177619211870734_another-powerpoint2.pptx#@En sprint för PO2.pptx",
            };

            var expectedResults = new List<SendEmailWithAttachmentsAfterSubmissionActor.DownloadFile>()
            {
                new SendEmailWithAttachmentsAfterSubmissionActor.DownloadFile()
                {
                    Name = "En sprint för PO.pptx",
                    Url = "/contentassets/819f6cc238fd416cbb58014465476c46/636177619211870734_another-powerpoint.pptx"
                },
                new SendEmailWithAttachmentsAfterSubmissionActor.DownloadFile()
                {
                    Name = "My Powerpoint.pptx",
                    Url = "/contentassets/819f6cc238fd416cbb58014465476c46/636177619211870734_powerpoint.pptx"
                },
                new SendEmailWithAttachmentsAfterSubmissionActor.DownloadFile()
                {
                    Name = "En sprint för PO2.pptx",
                    Url = "/contentassets/819f6cc238fd416cbb58014465476c46/636177619211870734_another-powerpoint2.pptx"
                },
                new SendEmailWithAttachmentsAfterSubmissionActor.DownloadFile()
                {
                    Name = "En sprint för PO3.pptx",
                    Url = "/contentassets/819f6cc238fd416cbb58014465476c46/636177619211870734_another-powerpoint3.pptx"
                },
                new SendEmailWithAttachmentsAfterSubmissionActor.DownloadFile()
                {
                    Name = "A third powerpoint.pptx",
                    Url = "/contentassets/819f6cc238fd416cbb58014465476c46/636177619211870734_a-third-powerpoint.pptx"
                }
            };

            var output = SendEmailWithAttachmentsAfterSubmissionActor.ParseUrls(input);

            var result = output.Count(downloadFile => expectedResults.Exists(x => x.Name == downloadFile.Name && x.Url == downloadFile.Url));
            Assert.AreEqual(5, result);
        }

        [TestMethod]
        public void ParseUrls_WithEmptyInput_ShouldReturnZeroDownloadFile()
        {
            var input = new List<string>()
            {
            };

            var output = SendEmailWithAttachmentsAfterSubmissionActor.ParseUrls(input);
            Assert.AreEqual(0, output.Count());
        }

        //[TestMethod]
        //public void DownloadFileGetAbsoluteUrl_WithHttpInput_ReturnsHttpOutput()
        //{
        //    var host = "http://mypage.se/Whatever/Path";
        //    var inputUri = new Uri(host);
        //    var myDownloadFile = new SendEmailWithAttachmentsAfterSubmissionActor.DownloadFile()
        //    {
        //        Name = "A third powerpoint.pptx",
        //        Url = "/contentassets/819f6cc238fd416cbb58014465476c46/636177619211870734_a-third-powerpoint.pptx"
        //    };
        //    var output = myDownloadFile.GetAbsoluteUrl(inputUri);
        //    var expectedResult = "http://mypage.se" + myDownloadFile.Url;
        //    Assert.AreEqual(expectedResult, output);
        //}

        //[TestMethod]
        //public void DownloadFileGetAbsoluteUrl_WithHttpsInput_ReturnsHttpsOutput()
        //{
        //    var host = "https://mypage.se";
        //    var inputUri = new Uri(host);
        //    var myDownloadFile = new SendEmailWithAttachmentsAfterSubmissionActor.DownloadFile()
        //    {
        //        Name = "A third powerpoint.pptx",
        //        Url = "/contentassets/819f6cc238fd416cbb58014465476c46/636177619211870734_a-third-powerpoint.pptx"
        //    };
        //    var output = myDownloadFile.GetAbsoluteUrl(inputUri);
        //    var expectedResult = host + myDownloadFile.Url;
        //    Assert.AreEqual(expectedResult, output);
        //}
    }
}
