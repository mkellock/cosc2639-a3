using System;
using System.Text.Json;
using Amazon.S3;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;

namespace api.Controllers;

public class Video
{
    public Guid ID { get; set; }
    public int Upvotes { get; set; }
    public int Downvotes { get; set; }
    public string? City { get; set; }
    public string? Region { get; set; }
    public string? Country { get; set; }
}

[ApiController]
[Route("[controller]")]
public class VideoController : ControllerBase
{
    private readonly ILogger<VideoController> _logger;

    public VideoController(ILogger<VideoController> logger)
    {
        _logger = logger;
    }

    [HttpGet("get_videos")]
    public List<Video> GetVideos()
    {
        // Set the CORS headers
        HttpContext.Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type,X-Amz-Date,Authorization,X-Api-Key,x-requested-with");
        HttpContext.Response.Headers.Add("Access-Control-Allow-Origin", "*");
        HttpContext.Response.Headers.Add("Access-Control-Allow-Methods", "POST,GET,OPTIONS");

        // Open a database connection
        using var conn = new MySqlConnection(Environment.GetEnvironmentVariable("CONNECTION_STRING"));
        conn.Open();

        // Save the new video record
        using var cmd = new MySqlCommand();
        cmd.Connection = conn;

        // Create the database if it doesn't exist
        _logger.LogInformation("Checking DB");
        cmd.CommandText = "SELECT `id`, `upvotes`, `downvotes`, `city`, `region`, `country` FROM `cosc26393`.`videos` WHERE `is_processing` = 0 ORDER BY `upload_date` DESC;";
        var reader = cmd.ExecuteReader();

        // Create a list to return
        List<Video> videos = new();

        // Add the videos to the list
        while (reader.Read())
        {
            videos.Add(new()
            {
                ID = reader.GetGuid(0),
                Upvotes = reader.GetInt32(1),
                Downvotes = reader.GetInt32(2),
                City = reader.GetString(3),
                Region = reader.GetString(4),
                Country = reader.GetString(5)
            });
        }

        // Return the videos
        return videos;
    }

    [HttpPost("upvote/{ID}")]
    public void Upvote(Guid ID)
    {
        // Set the CORS headers
        HttpContext.Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type,X-Amz-Date,Authorization,X-Api-Key,x-requested-with");
        HttpContext.Response.Headers.Add("Access-Control-Allow-Origin", "*");
        HttpContext.Response.Headers.Add("Access-Control-Allow-Methods", "POST,GET,OPTIONS");

        // Open a database connection
        using var conn = new MySqlConnection(Environment.GetEnvironmentVariable("CONNECTION_STRING"));
        conn.Open();

        // Save the new video record
        using var cmd = new MySqlCommand();
        cmd.Connection = conn;

        // Create the database if it doesn't exist
        _logger.LogInformation("Checking DB");
        cmd.CommandText = "UPDATE `cosc26393`.`videos` SET `upvotes` = `upvotes` + 1 WHERE id = '" + ID.ToString() + "';";
        cmd.ExecuteNonQuery();
    }

    [HttpPost("downvote/{ID}")]
    public void Downvote(Guid ID)
    {
        // Set the CORS headers
        HttpContext.Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type,X-Amz-Date,Authorization,X-Api-Key,x-requested-with");
        HttpContext.Response.Headers.Add("Access-Control-Allow-Origin", "*");
        HttpContext.Response.Headers.Add("Access-Control-Allow-Methods", "POST,GET,OPTIONS");

        // Open a database connection
        using var conn = new MySqlConnection(Environment.GetEnvironmentVariable("CONNECTION_STRING"));
        conn.Open();

        // Save the new video record
        using var cmd = new MySqlCommand();
        cmd.Connection = conn;

        // Create the database if it doesn't exist
        _logger.LogInformation("Checking DB");
        cmd.CommandText = "UPDATE `cosc26393`.`videos` SET `downvotes` = `downvotes` + 1 WHERE id = '" + ID.ToString() + "';";
        cmd.ExecuteNonQuery();
    }

    [HttpPost("upload")]
    [RequestSizeLimit(100_000_000)]
    public void Upload([FromForm] string IP, [FromForm] string contents)
    {
        // Set the CORS headers
        HttpContext.Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type,X-Amz-Date,Authorization,X-Api-Key,x-requested-with");
        HttpContext.Response.Headers.Add("Access-Control-Allow-Origin", "*");
        HttpContext.Response.Headers.Add("Access-Control-Allow-Methods", "POST,GET,OPTIONS");

        // Unique identifier for the video
        Guid videoId = Guid.NewGuid();

        // Decode the Base64 file and load into a bitmap
        var fileContents = Convert.FromBase64String(contents[(contents.IndexOf("base64,") + 7)..]);

        // Upload the file
        _logger.LogInformation("Saving file");
        AmazonS3Client s3Client = new(Amazon.RegionEndpoint.APSoutheast2);
        s3Client.PutObjectAsync(
            new()
            {
                BucketName = Environment.GetEnvironmentVariable("BUCKET_NAME"),
                Key = string.Concat("uploads/", videoId.ToString(), ".mp4"),
                InputStream = new MemoryStream(fileContents)
            }
        ).Wait();

        // Grab the IP address details
        _logger.LogInformation("Retrieving IP info");
        HttpClient client = new();
        string ipInfo = client
            .GetAsync("https://ipinfo.io/" + IP)
            .Result
            .Content
            .ReadAsStringAsync()
            .Result;
        JsonDocument result = JsonDocument.Parse(ipInfo);

        // Open a database connection
        _logger.LogInformation("Connecting to DB server");
        using var conn = new MySqlConnection(Environment.GetEnvironmentVariable("CONNECTION_STRING"));
        conn.Open();

        // Save the new video record
        using var cmd = new MySqlCommand();
        cmd.Connection = conn;

        // Create the database if it doesn't exist
        _logger.LogInformation("Checking DB");
        cmd.CommandText = "CREATE DATABASE IF NOT EXISTS `cosc26393`;";
        cmd.ExecuteNonQuery();

        // Create the table if it doesn't exist
        _logger.LogInformation("Checking table");
        cmd.CommandText = "CREATE TABLE IF NOT EXISTS `cosc26393`.`videos` (" +
            "`id` VARCHAR(36) NOT NULL DEFAULT 'UUID()', " +
            "`ip` NVARCHAR(16) NOT NULL, " +
            "`city` NVARCHAR(255) NULL, " +
            "`region` NVARCHAR(255) NULL, " +
            "`country` VARCHAR(2) NULL, " +
            "`upload_date` DATETIME NOT NULL DEFAULT NOW(), " +
            "`upvotes` INT NOT NULL DEFAULT 0, " +
            "`downvotes` INT NOT NULL DEFAULT 0, " +
            "`is_processing` TINYINT NOT NULL DEFAULT 1, " +
            "PRIMARY KEY (`id`));";
        cmd.ExecuteNonQuery();

        // Insert the video record
        _logger.LogInformation("Inserting record");
        cmd.CommandText = "INSERT INTO `cosc26393`.`videos` (id, ip, city, region, country) " +
            "VALUES ('" +
            videoId.ToString() + "', '" +
            IP.Replace("'", "''") + "', '" +
            NullToEmpty(result.RootElement.GetProperty("city").GetString()).Replace("'", "''") + "', '" +
            NullToEmpty(result.RootElement.GetProperty("region").GetString()).Replace("'", "''") + "', '" +
            NullToEmpty(result.RootElement.GetProperty("country").GetString()).Replace("'", "''") + "')";
        cmd.ExecuteNonQuery();

        // Close the database connection
        conn.Close();
    }

    private static string NullToEmpty(string? input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return string.Empty;
        }

        return input;
    }
}