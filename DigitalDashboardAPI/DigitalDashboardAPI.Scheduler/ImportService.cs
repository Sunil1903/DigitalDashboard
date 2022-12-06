using DigitalDashboard.DAL.DTO;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using MongoDB.Bson;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using SharpCompress.Common;
using DigitalDashboard.DAL.Models;
using DocumentFormat.OpenXml.Bibliography;
using Irony;

namespace DigitalDashboardAPI.Scheduler
{
    public class ImportService
    {
        private readonly string? baseURL = ConfigurationManager.AppSettings.Get("BaseAPI");
        private readonly string? sourceFileDirectory = ConfigurationManager.AppSettings.Get("SourceFileDirectory");
        private readonly string? logTTL = ConfigurationManager.AppSettings.Get("LogTTL");       
        private readonly string? logDirectory = ConfigurationManager.AppSettings.Get("LogFileDirectory");

        // HttpClient is intended to be instantiated once per application,
        // rather than per-use.
        static readonly HttpClient client = new HttpClient();

        #region FileProcessing
        public async Task<Response> FileProcessing()
        {
            Console.WriteLine($"  ACTION: Searching file from the shared directory or location ...");
            Response response = new();
            string exMessage = string.Empty;
            try
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(sourceFileDirectory);
                if (!directoryInfo.Exists)
                {
                    exMessage = $"LOCATION: {directoryInfo.FullName}, File or directory not found.";
                    response.IsSuccess = false;
                    response.Message = exMessage;
                    var url = WriteLog("FAILURE", exMessage);
                    return response;
                }
                var filesInDirectory = directoryInfo.GetFileSystemInfos().ToList().OrderBy(x => x.LastWriteTimeUtc).Last();
                var file = (from f in directoryInfo.GetFiles()
                            orderby f.LastWriteTime descending
                            select f).First();

                if (File.Exists(file.ToString()))
                {
                    Console.WriteLine($"  ACTION: File found.");
                    Console.WriteLine($"  ACTION: FILE: {file}");
                    response = await Importing(file.ToString());
                }
                else
                {
                    exMessage = @"File not found at the desired location.";
                    response.IsSuccess = false;
                    response.Message = exMessage;
                    var url = WriteLog("FAILURE", exMessage);
                    return response;
                }
            }
            catch (Exception e)
            {
                exMessage = e.Message;
                response.IsSuccess = false;
                response.Message = exMessage;
                var url = WriteLog("EXCEPTION", exMessage);
                //throw;
            }
            return response;
        }
        #endregion

        #region Implementation_Importing
        private async Task<Response> Importing(string filePath)
        {
            Console.WriteLine($"  ACTION: Creating a request for the JobManager API.");
            Response response = new();
            string fileName = Path.GetFileName(filePath);
            try
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                string requestUri = baseURL + "/digitaldashboard/DataImport/ExcelToDB";
                using var content = new MultipartFormDataContent();
                using var fileStream = File.OpenRead(filePath);
                content.Add(new StreamContent(fileStream), "file", fileName);
                
                Console.WriteLine($"  ACTION: API Path {requestUri}");
                Console.WriteLine($"  ACTION: Calling the JobManager API ...");
                using HttpResponseMessage response1 = await client.PostAsync(requestUri, content);
                response1.EnsureSuccessStatusCode();
                string responseBody = await response1.Content.ReadAsStringAsync();

                Response? apiResponse = JsonConvert.DeserializeObject<Response>(responseBody);
                if (apiResponse != null)
                {
                    response.IsSuccess = apiResponse.IsSuccess;
                    response.Message = $"FILENAME: {fileName}, {apiResponse.Message}";
                    if (response.IsSuccess)
                    {
                        var url = WriteLog("SUCCESS", response.Message);
                    }
                    else
                    {
                        var url = WriteLog("FAILURE", response.Message);
                    }
                }
            }
            catch (Exception e)
            {               
                response.IsSuccess = false;
                response.Message = $"FILENAME: {fileName}, {e.Message}";
                var url = WriteLog("EXCEPTION", response.Message);
            }

            return response;
        }
        #endregion

        #region Implementation_WriteLog
        // Logging..
        private async Task<Uri?> WriteLog(string responseStatus, string message)
        {
            Uri? uri = null;
            try
            {
                // Writing log to text file
                WriteLogInTextFile(responseStatus, message);

                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                string requestUri = baseURL + "/digitaldashboard/Log/Logging";

                // Log collection expiration time (in hours)
                int duration = 10; // Default duration : 10 hours
                if (!string.IsNullOrEmpty(logTTL))
                {
                    duration = Convert.ToInt32(logTTL);
                }

                Log content = new Log
                {
                    Status = responseStatus,
                    LogTime = DateTime.Now,
                    Message = message,
                    ExpireAt = DateTime.Now.AddHours(duration)
                };

                HttpResponseMessage response1 = await client.PostAsJsonAsync(requestUri, content);
                response1.EnsureSuccessStatusCode();

                // return URI of the created resource.
                uri = response1.Headers.Location;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                WriteLogInTextFile("EXCEPTION", e.Message);
            }
            return uri;
        }
        #endregion

        #region Implementation_WriteLogInTextFile
        private void WriteLogInTextFile(string status, string message)
        {
            try
            {
                string? logDirectoryPath = logDirectory;

                if (!string.IsNullOrEmpty(logDirectoryPath))
                {
                    bool exists = System.IO.Directory.Exists(logDirectoryPath);

                    if (!exists)
                    {
                        System.IO.Directory.CreateDirectory(logDirectoryPath);
                    }

                    string path = logDirectoryPath + "\\Log_" + DateTime.Now.ToString("ddMMyyyy") + ".txt";

                    if (!File.Exists(path))
                    {
                        // Create a file to write to.
                        using StreamWriter sw = File.CreateText(path);
                        WriteLogInTextFile(status, message, sw);
                    }
                    else
                    {
                        using StreamWriter sw = File.AppendText(path);
                        WriteLogInTextFile(status, message, sw);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                //throw;
            }
            
        }

        private void WriteLogInTextFile(string status, string message, StreamWriter sw)
        {
            sw.Write($"\r\nLog: {DateTime.Now.ToLongDateString()} {DateTime.Now.ToLongTimeString()}");
            sw.WriteLine("-------------------------------");
            sw.WriteLine($"Status: {status}");
            sw.WriteLine($"Message: {message}");
            sw.WriteLine("-------------------------------");
            Console.WriteLine();
        }
        #endregion

        //// GET
        //private async Task<Response> Importing(string filePath)
        //{
        //    Console.WriteLine($"  ACTION: Creating a request to the JobManager API.");
        //    Response response = new();
        //    try
        //    {
        //        UriBuilder builder = new UriBuilder(baseURL + "/digitaldashboard/DataImport/ImportDataUsingFilePath")
        //        {
        //            Query = "filePath=" + filePath
        //        };

        //        client.DefaultRequestHeaders.Accept.Clear();
        //        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        //        using HttpResponseMessage response1 = await client.GetAsync(builder.Uri);
        //        response1.EnsureSuccessStatusCode();
        //        string responseBody = await response1.Content.ReadAsStringAsync();

        //        Console.WriteLine($"  ACTION: Calling the JobManager API.");
        //        // Above three lines can be replaced with new helper method below
        //        //string responseBody = await client.GetStringAsync(builder.Uri);

        //        Response? apiResponse = JsonConvert.DeserializeObject<Response>(responseBody);
        //        if (apiResponse != null)
        //        {
        //            response.IsSuccess = apiResponse.IsSuccess;
        //            response.Message = apiResponse.Message;
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        response.IsSuccess = false;
        //        response.Message = e.Message;
        //    }

        //    return response;
        //}

        // POST

    }
}
