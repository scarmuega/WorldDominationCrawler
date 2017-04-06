using System;
using Microsoft.Extensions.CommandLineUtils;
using Amazon.S3;

namespace WorldDominationCrawler
{
    internal static class CliExtensions
    {
        public static int? ParseIntOption(this CommandOption cmdOption)
        {
            if (!cmdOption.HasValue()) return null;
            
            if (String.IsNullOrWhiteSpace(cmdOption.Value())) return null;

            return Int32.Parse(cmdOption.Value());
        }
    }

    internal static class AwsExtensions
    {
        public static bool IsSecurityException(this AmazonS3Exception exception)
        {
            return (exception.ErrorCode != null) &&
                    (exception.ErrorCode.Equals("InvalidAccessKeyId") ||
                    exception.ErrorCode.Equals("InvalidSecurity"));
        }
    }
}