using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Runtime.Serialization.Json;
using System.Text;
using DotNetCommons;
using Foam.API.Attributes;
using Foam.API.Files;

namespace Foam.API.Commands
{
    [ShortDescription("List the contents of the file buffer to a target output.")]
    [LongDescription("List sends a list of what's currently in the filebuffer to a specific destination; by default " +
                     "the screen, but optionally also an email address or an HTTP post. The POST data will be a JSON structure " +
                     "containing the job name and all the files enclosed.")]
    public class ListCommand : ICommand
    {
        public class JsonPostFile
        {
            public string Filename { get; set; }
            public int Length { get; set; }
            public DateTimeOffset CreationTime { get; set; }
            public DateTimeOffset ModificationTime { get; set; }
        }

        public class JsonPost
        {
            public string JobName { get; set; }
            public string MachineName { get; set; }
            public DateTimeOffset Timestamp { get; set; }

            public List<JsonPostFile> Files { get; } = new List<JsonPostFile>();
        }

        [PropertyDescription("Optional file mask specifies what files to compress.")]
        public string Mask { get; set; }
        [PropertyDescription("URL to post to, if an HTTP transaction is desired.")]
        public Uri Url { get; set; }
        [PropertyDescription("From email address, if an email is desired. If left blank, the default app.settings email is used.")]
        public string FromEmail { get; set; }
        [PropertyDescription("Destination email address, if an email is desired.")]
        public string ToEmail { get; set; }

        public void Initialize()
        {
        }

        public void Execute(JobRunner runner)
        {
            var files = runner.FileBuffer.SelectFiles(Mask).ToList();

            if (!files.Any())
            {
                Logger.Log("List: No files to list.");
                return;
            }

            if (Url == null && string.IsNullOrEmpty(ToEmail))
            {
                ListToScreen(files);
                return;
            }

            if (Url != null)
                ListToHttp(files, runner.JobName, Url);

            if (!string.IsNullOrEmpty(ToEmail))
                ListToEmail(files, runner.JobName, ToEmail, FromEmail ?? ConfigurationManager.AppSettings["default-email"]);
        }

        private void ListToHttp(List<FileItem> files, string jobName, Uri url)
        {
            var data = new JsonPost
            {
                JobName = jobName,
                MachineName = Environment.MachineName,
                Timestamp = DateTimeOffset.Now
            };

            foreach(var file in files)
                data.Files.Add(new JsonPostFile
                {
                    Filename = file.Name,
                    Length = file.Length,
                    CreationTime = file.CreationTime,
                    ModificationTime = file.ModifiedTime
                });

            var serializer = new DataContractJsonSerializer(typeof(JsonPost));
            using (var mem = new MemoryStream())
            {
                serializer.WriteObject(mem, data);

                Logger.Log("HTTP posting list to " + url);

                using (var http = new WebClient())
                {
                    http.Headers[HttpRequestHeader.ContentType] = "application/json";
                    http.UploadData(url, "POST", mem.ToArray());
                }

                Logger.Debug("HTTP post completed.");
            }
        }

        private void ListToEmail(List<FileItem> files, string jobName, string toEmail, string fromEmail)
        {
            var buffer = new StringBuilder();

            buffer.AppendLine($"File contents of job {jobName} at {DateTime.Now:R}");
            buffer.AppendLine();

            foreach (var file in files)
                buffer.AppendLine(file.ToString());

            buffer.AppendLine();
            buffer.AppendLine($"{files.Count} files, {files.Sum(x => x.Length)} bytes.");

            Logger.Log("Sending email to " + toEmail);

            using (var smtp = new SmtpClient())
                smtp.Send(new MailMessage(fromEmail, toEmail)
                {
                    Subject = $"[foam] {jobName} on {Environment.MachineName}",
                    Body = buffer.ToString()
                });

            Logger.Debug("Email sent");
        }

        private void ListToScreen(List<FileItem> files)
        {
            foreach(var file in files)
                Logger.Log(file.ToString());

            Logger.Log($"{files.Count} files, {files.Sum(x => x.Length)} bytes.");
        }
    }
}
