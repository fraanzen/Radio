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

            scheduler.ScheduleReportage(
                scheduler.GetDay("today").AddHours(8), 
                TimeSpan.FromMinutes(30), 
                "Morning News", 
                "Local News", 
                "John Reporter"
            );

            scheduler.ScheduleLiveSession(
                scheduler.GetDay("today").AddHours(7), 
                TimeSpan.FromHours(1), 
                "Morning Show", 
                new List<string> { "Mike Host" }
            );

            scheduler.ScheduleLiveSession(
                scheduler.GetDay("today").AddHours(16), 
                TimeSpan.FromHours(2), 
                "Afternoon Talk", 
                new List<string> { "Sarah Host", "Bob Host" }
            );

            scheduler.ScheduleLiveSession(
                scheduler.GetDay("today").AddHours(20), 
                TimeSpan.FromHours(1), 
                "Evening Interview", 
                new List<string> { "Emma Host" },
                new List<string> { "Dr. Expert", "Author Jane" }
            );

            scheduler.ScheduleLiveSession(
                scheduler.GetDay("sunday").AddHours(10), 
                TimeSpan.FromHours(2), 
                "Sunday Brunch Show", 
                new List<string> { "Alex Host" },
                new List<string> { "Chef Maria" }
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
