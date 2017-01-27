﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using Castle.Core.Internal;
using EPiServer.Forms.Core;
using EPiServer.Forms.Core.Data;
using EPiServer.Forms.Core.Internal;
using EPiServer.Forms.Core.Models;
using EPiServer.Forms.Core.Models.Internal;
using EPiServer.Forms.Core.PostSubmissionActor;
using EPiServer.Forms.EditView;
using EPiServer.Forms.EditView.SpecializedProperties;
using EPiServer.Forms.Helpers.Internal;
using EPiServer.Forms.Implementation.Actors;
using EPiServer.Logging;
using EPiServer.ServiceLocation;

namespace Alloy.EpiFormsCustom.Actors
{
    /*This is a custom actor to send emails with uploaded files as attachments rather than links
    Most code here (except SendEmailWithAttachments) is copied from episerver forms original SendEmailAfterSubmissionActor.*/
    public class SendEmailWithAttachmentsAfterSubmissionActor : SendEmailAfterSubmissionActor, IUIPropertyCustomCollection
    {
        private static SmtpClient _smtpClient = new SmtpClient();
        private bool _sendMessageInHTMLFormat = false;
        private readonly Injected<PlaceHolderService> _placeHolderService;
        private readonly Injected<IFormDataRepository> _formDataRepository;
        protected Injected<IEPiServerFormsUIConfig> _formConfig;

        protected Injected<FormRepository> _formRepository;

        public override object Run(object input)
        {
            IEnumerable<EmailTemplateActorModel> model = this.Model as IEnumerable<EmailTemplateActorModel>;
            if (model == null || model.Count<EmailTemplateActorModel>() < 1)
                return (object)null;
            foreach (EmailTemplateActorModel emailConfig in model)
                this.SendMessage(emailConfig);
            return (object)null;
        }

        public virtual Type PropertyType
        {
            get
            {
                return typeof(PropertyEmailTemplateActorList);
            }
        }

        public SendEmailWithAttachmentsAfterSubmissionActor()
        {
            this._sendMessageInHTMLFormat = this._formConfig.Service.SendMessageInHTMLFormat;
        }

        private void SendMessage(EmailTemplateActorModel emailConfig)
        {
            if (string.IsNullOrEmpty(emailConfig.ToEmails))
                return;
            string[] strArray = emailConfig.ToEmails.SplitBySeparator(",");
            if (strArray == null || ((IEnumerable<string>)strArray).Count<string>() == 0)
                return;
            try
            {
                IEnumerable<PlaceHolder> subjectPlaceHolders = this.GetSubjectPlaceHolders((IEnumerable<FriendlyNameInfo>)null);
                IEnumerable<PlaceHolder> bodyPlaceHolders = this.GetBodyPlaceHolders((IEnumerable<FriendlyNameInfo>)null);
                string str = this._placeHolderService.Service.Replace(emailConfig.Subject, subjectPlaceHolders, false);
                string template1 = emailConfig.Body == null ? string.Empty : emailConfig.Body.ToHtmlString();
                Injected<PlaceHolderService> placeHolderService = this._placeHolderService;
                string content = placeHolderService.Service.Replace(template1, bodyPlaceHolders, false);
                MailMessage message = new MailMessage();
                message.Subject = str;
                message.Body = this.RewriteUrls(content);
                message.IsBodyHtml = this._sendMessageInHTMLFormat;
                if (!string.IsNullOrEmpty(emailConfig.FromEmail))
                {
                    MailMessage mailMessage = message;
                    placeHolderService = this._placeHolderService;
                    MailAddress mailAddress = new MailAddress(placeHolderService.Service.Replace(emailConfig.FromEmail, subjectPlaceHolders, false));
                    mailMessage.From = mailAddress;
                }
                foreach (string template2 in strArray)
                {
                    try
                    {
                        MailAddressCollection to = message.To;
                        placeHolderService = this._placeHolderService;
                        MailAddress mailAddress = new MailAddress(placeHolderService.Service.Replace(template2, bodyPlaceHolders, false));
                        to.Add(mailAddress);
                    }
                    catch (Exception ex)
                    {
                    }
                }

                bool sendEmailsWithAttachments = SendEmailWithAttachments(this.HttpRequestContext, this.SubmissionData, message);
                if (!sendEmailsWithAttachments)
                    _smtpClient.Send(message);
            }
            catch (Exception ex)
            {
                PostSubmissionActorBase._logger.Error("Failed to send e-mail: {0}", ex);
            }
        }

        public bool SendEmailWithAttachments(HttpRequestBase httpRequestContext, Submission submissionData, MailMessage message)
        {
            IEnumerable<FriendlyNameInfo> friendlyNameInfos = this._formRepository.Service.GetFriendlyNameInfos(this.FormIdentity, typeof(IExcludeInSubmission));
            IEnumerable<string> elementIds = friendlyNameInfos.Where(f => f.FormatType == FormatType.Link).Select(f => f.ElementId);
            IEnumerable<DownloadFile> uploadElementFiles = GetUrls(elementIds, SubmissionData.Data);

            if (uploadElementFiles.Any())
            {
                foreach (var file in uploadElementFiles)
                {
                    string fileName = file.Name;
                    HttpWebRequest req = (HttpWebRequest)WebRequest.Create(file.GetAbsoluteUrl(httpRequestContext));
                    using (HttpWebResponse HttpWResp = (HttpWebResponse)req.GetResponse())
                    using (Stream responseStream = HttpWResp.GetResponseStream())
                    {
                        MemoryStream ms = new MemoryStream();
                        responseStream.CopyTo(ms);
                        ms.Seek(0, SeekOrigin.Begin);
                        string mime = MimeMapping.GetMimeMapping(fileName);
                        Attachment attachment = new Attachment(ms, fileName, mime);
                        message.Attachments.Add(attachment);
                    }
                }
                _smtpClient.Send(message);
                message.Dispose();
                /*A MailMessage (IDisposable) contains multiple Attachments (IDisposable). Each attachment references a MemoryStream (IDisposable). 
                    * The MailMessage is disposed which in turn calls the Dispose method of all attachments which in turn call the Dispose method of 
                    * the memory streams. http://stackoverflow.com/questions/7318794/attaching-multiple-files-to-an-e-mail-from-database-image-column-in-net*/

                return true;
            }
            return false;
        }

        public IEnumerable<DownloadFile> GetUrls(IEnumerable<string> elementIds, IDictionary<string, object> submissionDataDict)
        {
            IEnumerable<string> uploadElementFiles = GetUploadElements(submissionDataDict, elementIds);
            return ParseUrls(uploadElementFiles);
        }

        public static IEnumerable<DownloadFile> ParseUrls(IEnumerable<string> uploadElementFiles)
        {
            var result = new List<DownloadFile>();
            foreach (var uploadElementFile in uploadElementFiles)
            {
                var files = uploadElementFile.Split('|');
                foreach (var file in files)
                {
                    var fileparts = file.Split(new string[] { "#@" }, StringSplitOptions.None);
                    var downloadFile = new DownloadFile()
                    {
                        Name = fileparts.Last(),
                        Url = fileparts.First()
                    };
                    result.Add(downloadFile);
                }
            }
            return result;
        }

        public static IEnumerable<string> GetUploadElements(IDictionary<string, object> submissionDataDict, IEnumerable<string> elementIds)
        {
            return submissionDataDict.Where(o => elementIds.Any(elementId => elementId == o.Key) && o.Value.ToString() != string.Empty).Select(o => o.Value.ToString());
        }

        public static string GetFileName(string url)
        {
            Uri uri = new Uri(url);
            var name = Path.GetFileName(uri.LocalPath);
            name = name.Remove(0, name.IndexOf('_') + 1); //Episerver will add an id and underscore before original filename
            return name;
        }

        public class DownloadFile
        {
            public string Name { get; set; }
            public string Url { get; set; }
            public string GetAbsoluteUrl(HttpRequestBase httpRequestBase)
            {
                var isSecure = false;
                try
                {
                    isSecure = httpRequestBase.IsSecureConnection;
                }
                catch
                {
                    //httpRequestBase.IsSecureConnection can throw Argument exceptions 
                }
                return (isSecure ? "https://" : "http://") + httpRequestBase.Url.Host + ":" + httpRequestBase.Url.Port + this.Url;

            }
        }
    }
}
