using DigitalDashboard.DAL.DTO;
using DigitalDashboardAPI.Scheduler;
using System;
using System.Net.Http.Headers;
using System.Net.Http.Json;

Console.WriteLine("-----------------------");
Console.WriteLine("JOB MANAGER: Scheduler");
Console.WriteLine("-----------------------");
Console.WriteLine($"\n  ACTION: Process starting ...");

// Import Service
ImportService importService = new();
Response response = await importService.FileProcessing();

// Print API Response
Console.WriteLine($"\n    RESULT:");
Console.WriteLine("    -------");

if (response != null)
{
    if (response.IsSuccess)
    {
        Console.WriteLine($"    Status: SUCCESS.");
    }
    else
    {
        Console.WriteLine($"    Status: FAILURE.");
    }
    Console.WriteLine($"    Message: {response.Message}");
}

Console.WriteLine($"\n  ACTION: Stopped.");
Thread.Sleep(10000);
//Console.ReadLine();