using System;
using System.IO;
using System.Linq;
using Amazon;
using DotNetCommons;
using DotNetCommons.WinForms.Graphics;
using Foam.API;
using Foam.API.Attributes;
using Foam.API.Commands;
using Foam.API.Files;
using Foam.Extensions.AWS.Classes;

namespace Foam.Extensions.AWS.Commands
{
    [ShortDescription("Uses AWS Rekognition to automatically tag photos with faces and objects.")]
    [LongDescription("Uses the Amazon AWS Rekognition service to put EXIF tags in pictures, " +
                     "based on facial data and object recognition. For facial detection, it's required " +
                     "that you've uploaded facial data already using the AWS command-line tools.")]
    public class AwsRekognizeCommand : ICommand
    {
        [PropertyDescription("File mask to operate upon.")]
        public string Mask { get; set; }
        [PropertyDescription("AWS Rekognition access key.")]
        public string AccessKey { get; set; }
        [PropertyDescription("AWS Rekognition secret key.")]
        public string SecretKey { get; set; }
        [PropertyDescription("AWS Rekognition collection, for optional face detection.")]
        public string Collection { get; set; }
        [PropertyDescription("Force processing, even if tags already present in image.")]
        public bool Force { get; set; }
        [PropertyDescription("AWS Region Endpoint, default is USEast1.")]
        public RegionEndpoint Region { get; set; } = RegionEndpoint.USEast1;

        public void Initialize()
        {
        }

        public void Execute(JobRunner runner)
        {
            var rekognizer = new Rekognizer(AccessKey, SecretKey, Region);
            foreach (var file in runner.FileBuffer.SelectFiles(Mask))
                DoDetect(rekognizer, file);
        }

        private void DoDetect(Rekognizer rekognizer, FileItem file)
        {
            using (var stream = file.GetStream(false))
            using (var exif = new ExifImage(stream))
            {
                var hasTags = !string.IsNullOrEmpty(exif.TagAsText);
                if (hasTags && !Force)
                {
                    Logger.Log($"AWS Rekognition: Skipping {file.Name}, already has tags.");
                    return;
                }
            }

            Logger.Log("AWS Rekognition: Processing " + file.Name);
            var result = rekognizer.Detect(Collection, file);
            if (!result.Any())
                return;

            Logger.Log("AWS Rekognition: Found tags: " + string.Join(", ", result));

            using (var stream = (MemoryStream)file.GetStream(true))
            {
                using (var exif = new ExifImage(stream))
                {
                    exif.Tags = result.ToArray();
                    exif.Save();
                }

                file.SetData(stream.ToArray());
            }
        }
    }
}
