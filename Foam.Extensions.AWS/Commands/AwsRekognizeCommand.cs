using System;
using Foam.API;
using Foam.API.Attributes;
using Foam.API.Commands;

namespace Foam.Extensions.AWS.Commands
{
    [ShortDescription("Uses AWS Rekognition to automatically tag photos with faces and objects.")]
    [LongDescription("Uses the Amazon AWS Rekognition service to put EXIF tags in pictures, " +
                     "based on facial data and object recognition. Requires that you've uploaded " +
                     "facial data already using the AWS command-line tools.")]
    public class AwsRekognizeCommand : ICommand
    {
        public void Initialize()
        {
            throw new NotImplementedException();
        }

        public void Execute(JobRunner runner)
        {
            throw new NotImplementedException();
        }
    }
}
