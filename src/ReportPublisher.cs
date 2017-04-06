using System;
using System.Threading.Tasks;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;

namespace WorldDominationCrawler
{
    internal static class ReportPublisher
    {
        public static async Task<Guid?> UploadToS3Async(ReportData data)
        {
            var reportGuid = Guid.NewGuid();
            var jsonData = data.ToJson();
            var credentials = new AnonymousAWSCredentials();

            try
            {
                using (var client = new AmazonS3Client(credentials, RegionEndpoint.USEast1))
                {
                    var request = new PutObjectRequest()
                    {
                        ContentBody = jsonData,
                        ContentType = "application/json",
                        BucketName = "world-crawler",
                        Key = String.Format("{0}.json", reportGuid.ToString()),
                    };

                    var response = await client.PutObjectAsync(request);
                    return reportGuid;
                }
            }
            catch (AmazonS3Exception amazonS3Exception)
            {
                if (amazonS3Exception.IsSecurityException())
                {
                    throw new Exception("AWS S3 object can be uploaded for security issues");
                }
                
                throw amazonS3Exception;
            }
        }
    }
}