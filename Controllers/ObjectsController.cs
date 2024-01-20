﻿using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using Crud_S3.Models;
using Microsoft.AspNetCore.Mvc;

namespace Crud_S3.Controllers
{
    [Route("api/objects")]
    [ApiController]
    public class ObjectsController : ControllerBase
    {
        private readonly IAmazonS3 _s3Client;

        public ObjectsController(IAmazonS3 s3Client) => _s3Client = s3Client;

        [HttpPost]
        public async Task<IActionResult> UploadFileAsync(IFormFile file, string bucketName, string? prefix)
        {
            var bucketExists = await AmazonS3Util.DoesS3BucketExistV2Async(_s3Client, bucketName);
            if (!bucketExists)
                return NotFound($"Bucket {bucketName} does not exist.");

            var request = new PutObjectRequest()
            {
                BucketName = bucketName,
                Key = string.IsNullOrEmpty(prefix) ? file.FileName : $"{prefix?.TrimEnd('/')}/{file.FileName}",
                InputStream = file.OpenReadStream()
            };
            request.Metadata.Add("Content-Type", file.ContentType);
            await _s3Client.PutObjectAsync(request);

            return Ok($"File {prefix}/{file.FileName} uploaded to S3 successfully!");
        }

        [HttpGet]
        public async Task<IActionResult> GetAllObjectsAsync(string bucketName, string? prefix)
        {
            var bucketExists = await AmazonS3Util.DoesS3BucketExistV2Async(_s3Client, bucketName);
            if (!bucketExists)
                return NotFound($"Bucket {bucketName} does not exist.");

            var request = new ListObjectsV2Request()
            {
                BucketName = bucketName,
                Prefix = prefix
            };

            var result = await _s3Client.ListObjectsV2Async(request);
            var s3Objects = result.S3Objects.Select(o =>
            {
                var urlRequest = new GetPreSignedUrlRequest()
                {
                    BucketName = bucketName,
                    Key = o.Key,
                    Expires = DateTime.UtcNow.AddMinutes(1)
                };
                return new S3ObjectDto()
                {
                    Name = o.Key.ToString(),
                    PresignedUrl = _s3Client.GetPreSignedURL(urlRequest)
                };
            });
            return Ok(s3Objects);
        }

        [HttpGet("preview")]
        public async Task<IActionResult> GetObjectByKeyAsync(string bucketName, string key)
        {
            var bucketExists = await AmazonS3Util.DoesS3BucketExistV2Async(_s3Client, bucketName);
            if (!bucketExists)
                return NotFound($"Bucket {bucketName} does not exist.");

            var s3Object = await _s3Client.GetObjectAsync(bucketName, key);

            return File(s3Object.ResponseStream, s3Object.Headers.ContentType);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteObjectAsync(string bucketName, string key)
        {
            var bucketExists = await AmazonS3Util.DoesS3BucketExistV2Async(_s3Client, bucketName);
            if (!bucketExists)
                return NotFound($"Bucket {bucketName} does not exist.");

            await _s3Client.DeleteObjectAsync(bucketName, key);

            return NoContent();
        }

    }
}
