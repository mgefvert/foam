using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Amazon;
using Amazon.Rekognition;
using Amazon.Rekognition.Model;
using Amazon.Runtime;
using DotNetCommons;
using DotNetCommons.WinForms.Graphics;
using Foam.API.Files;
using Image = Amazon.Rekognition.Model.Image;

namespace Foam.Extensions.AWS.Classes
{
    public class Rekognizer
    {
        private readonly AmazonRekognitionClient _client;

        internal class Tag
        {
            public string Name;
            public int Confidence;
            public override string ToString() => Name + "(" + Confidence + ")";

            public Tag(string name, float confidence)
            {
                Name = name.Trim().ToLower().Replace(' ', '-');
                Confidence = (int) confidence;
            }
        }

        private class TagComparer : IEqualityComparer<Tag>
        {
            public bool Equals(Tag x, Tag y) => string.Equals(x.Name, y.Name, StringComparison.InvariantCultureIgnoreCase);
            public int GetHashCode(Tag obj) => obj.Name.GetHashCode();
        }

        public Rekognizer(string accessKey, string secretKey, RegionEndpoint region)
        {
            _client = new AmazonRekognitionClient(new BasicAWSCredentials(accessKey, secretKey), region);
        }

        public List<string> Detect(string collection, FileItem file)
        {
            var img = LoadPicture(file);
            var tags = new HashSet<Tag>(new TagComparer());

            // Process labels

            var labels = DetectLabels(img, 90).Select(x => new Tag(x.Name, x.Confidence)).ToList();
            Logger.Debug("Labels = " + string.Join(", ", labels));
            tags.AddRangeIfNotNull(labels);

            // Process faces

            var faces = DetectFaces(img);
            var i = 0;
            foreach (var face in faces.Where(x => x.Confidence >= 90))
            {
                Logger.Debug($"Face {++i}...");

                var emotions = face.Emotions
                    .Where(x => x.Confidence >= 90)
                    .Select(x => new Tag("emotion-" + x.Type.Value, x.Confidence))
                    .ToList();
                Logger.Debug("Emotions = " + string.Join(", ", emotions));
                tags.AddRangeIfNotNull(emotions);

                var gender = face.Gender.Confidence >= 90
                    ? new Tag(face.Gender.Value.Value, face.Gender.Confidence)
                    : null;
                Logger.Debug("Gender = " + gender);
                tags.AddIfNotNull(gender);

                if (!string.IsNullOrEmpty(collection))
                {
                    // Recognize single face - if Collection is assigned, of course

                    var faceImg = CropPicture(img, face.BoundingBox);
                    try
                    {
                        var bestMatch = DetectFace(collection, faceImg, 70);
                        var person = bestMatch != null
                            ? new Tag("@" + bestMatch.Face.ExternalImageId, bestMatch.Similarity)
                            : null;
                        Logger.Debug("Person = " + person);
                        tags.AddIfNotNull(person);
                    }
                    catch (Exception ex)
                    {
                        Logger.Log($"AWS error while processing {file.Name}, face {i}: {ex.Message}");
                    }
                }
            }

            return tags.Select(x => x.Name).OrderBy(x => x).ToList();
        }

        private static Image CropPicture(Image img, BoundingBox bounds)
        {
            img.Bytes.Position = 0;
            using (var bitmap = System.Drawing.Image.FromStream(img.Bytes))
            {
                var h = bitmap.Height;
                var w = bitmap.Width;

                var rect = new RectangleF(bounds.Left * w, bounds.Top * h, bounds.Width * w, bounds.Height * h);
                var processor = new ImageProcessor(bitmap);
                processor.Crop(rect);
                processor.ScaleMin(new Size(128, 128));

                var result = new Image { Bytes = new MemoryStream() };
                using (var stream = processor.AsJpeg())
                    stream.CopyTo(result.Bytes);

                return result;
            }
        }

        private List<FaceDetail> DetectFaces(Image image)
        {
            var result = _client.DetectFaces(new DetectFacesRequest
            {
                Image = image,
                Attributes = new List<string> { "ALL" }
            });

            return result.FaceDetails;
        }

        private FaceMatch DetectFace(string collection, Image image, int minConfidence)
        {
            var people = _client.SearchFacesByImage(new SearchFacesByImageRequest
            {
                CollectionId = collection,
                FaceMatchThreshold = minConfidence,
                Image = image
            });

            return people.FaceMatches.OrderByDescending(x => x.Similarity).FirstOrDefault();
        }

        private List<Label> DetectLabels(Image image, int minConfidence)
        {
            return _client.DetectLabels(new DetectLabelsRequest
            {
                Image = image,
                MinConfidence = minConfidence
            }).Labels;
        }

        private static Image LoadPicture(FileItem item)
        {
            var bitmap = System.Drawing.Image.FromStream(item.GetStream(false));
            var imageProcessor = new ImageProcessor(bitmap);

            if (imageProcessor.Width > 800 || imageProcessor.Height > 600)
                imageProcessor.ScaleMin(new Size(800, 600));

            var result = new Image { Bytes = new MemoryStream() };
            using (var stream = imageProcessor.AsJpeg())
            {
                stream.CopyTo(result.Bytes);
            }
            result.Bytes.Position = 0;

            return result;
        }
    }
}
