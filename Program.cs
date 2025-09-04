using System;
using Radio.Models;
using Radio.Services;

namespace Radio
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Radio Scheduling System ===\n");

            SchedulerService scheduler = new SchedulerService();

            DateTime today = DateTime.Today;
            
            scheduler.ScheduleReportage(
                today.AddHours(8), 
                TimeSpan.FromMinutes(30), 
                "Morning News", 
                "Local News", 
                "John Reporter"
            );

            scheduler.ScheduleLiveSession(
                today.AddHours(7), 
                TimeSpan.FromHours(1), 
                "Morning Show", 
                new List<string> { "Mike Host" }
            );

            scheduler.ScheduleLiveSession(
                today.AddHours(16), 
                TimeSpan.FromHours(2), 
                "Afternoon Talk", 
                new List<string> { "Sarah Host", "Bob Host" }
            );

            scheduler.ScheduleLiveSession(
                today.AddHours(20), 
                TimeSpan.FromHours(1), 
                "Evening Interview", 
                new List<string> { "Emma Host" },
                new List<string> { "Dr. Expert", "Author Jane" }
            );

            scheduler.FillWithMusic();

            Console.WriteLine(scheduler.GetScheduleOverview());
            
            Console.WriteLine();
            Console.WriteLine(scheduler.GetStudioSummary());

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
}
