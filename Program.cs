using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper;
namespace DataProcessor
{
    class Genre
    {
        public int count { get; set; }
        public Dictionary<string, int> years { get; set;}
    }
    class YearAverage
    {
        public int count { get; set; }
        public int total { get; set; }
    }
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                Process(args);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("IMDB Dataset analyser\n");
                
                Console.Title = "IMDB Dataset analyser";
                Thread.Sleep(200);
                Console.WriteLine("Please enter the path of the IMDB dataset .tsv file to process:");
                
                string result = Console.ReadLine();
                Console.WriteLine();
                Process(new string[] { result });
            }

            Console.WriteLine("\nPress any key to exit");
            Console.ReadKey();
        }
        static void Process(string[] args)
        {
           // HackermanIntroBullshit(args);

            int counter = 1;
            string line;

            //How the IMDB data is formatted:
            //tconst,titleType,primaryTitle,originalTitle,isAdult,startYear,endYear,runtimeMinutes,genres
            Console.WriteLine("Opening dataset");
            Thread.Sleep(500);
            StreamReader file = new System.IO.StreamReader(args[0]);
            double delay = 200;

            Dictionary<string, Genre> genreList = new Dictionary<string, Genre>();
            Dictionary<string, Genre> primarygenreList = new Dictionary<string, Genre>();


            Dictionary<string, YearAverage> averageRuntime = new Dictionary<string, YearAverage>();
            int moviecount = 0;
            int overallcount = 0;
            List<string> AllYears = new List<string>();

            Console.WriteLine("Starting analysis loop...");
            while ((line = file.ReadLine()) != null)
            {
                counter++;
                overallcount++;
                var fields = line.Split('\t');
                string tconst = fields[0];

                string titleType = fields[1];
                if (titleType == "movie")
                {
                    string primaryTitle = fields[2];
                    string originalTitle = fields[3];
                    string isAdult = fields[4];
                    string startYear = fields[5];
                    string endYear = fields[5];
                    if (!AllYears.Contains(endYear))
                        AllYears.Add(endYear);


                    string runtimeMinutes = fields[7];
                    string[] genres = new string[3];


                    if (fields.Length >= 9)
                    {
                        genres = fields[8].Split(',');
                        if (!primarygenreList.ContainsKey(genres[0]))
                        {
                            primarygenreList.Add(genres[0], new Genre() { years = new Dictionary<string, int>() });
                        }

                        primarygenreList[genres[0]].count++;
                        if (!primarygenreList[genres[0]].years.ContainsKey(endYear))
                        {
                            primarygenreList[genres[0]].years.Add(endYear, 0);
                        }
                        primarygenreList[genres[0]].years[endYear]++;
                    }
                    if (runtimeMinutes != null && runtimeMinutes!=@"\N")
                    {
                        if (!averageRuntime.ContainsKey(endYear))
                            averageRuntime.Add(endYear, new YearAverage());
                        averageRuntime[endYear].count++;
                        averageRuntime[endYear].total += Convert.ToInt32(runtimeMinutes); ;
                    }

                    moviecount++;
                   
                    foreach (string g in genres)
                    {
                        if (g != null)
                        {
                            if (!genreList.ContainsKey(g))
                            {
                                genreList.Add(g, new Genre() { years = new Dictionary<string, int>() });
                            }

                            genreList[g].count++;
                            if (!genreList[g].years.ContainsKey(endYear))
                            {
                                genreList[g].years.Add(endYear, 0);
                            }
                            genreList[g].years[endYear]++;
                        }
                    }
                    if(counter > 10000)
                    {
                        counter = 0;
                        Console.Write("\rProcessed " + overallcount + " titles");
                    }
                    //  Console.WriteLine("Processing item " + counter + "/4879165 (" + tconst + " - " + primaryTitle + ")");
                }

            }
            Console.WriteLine("\nDataset analysis complete");
            Console.WriteLine("Analysed " + moviecount + " titles with tconst = 'movie'");

            Console.WriteLine("Getting year list");
            Thread.Sleep(100);
            string tsv = "genre";
            AllYears = AllYears.OrderBy(x => x).ToList();
            foreach (var year in AllYears)
            {
                tsv = tsv + "\t" + year;
            }
            string tsv2 = tsv;
            string tsv3 = tsv;
            Console.WriteLine("Creating genre rows");
            foreach (var genre in genreList.OrderBy(x => x.Value.count))
            {
                string peakYear = "";
                int peakCount = 0;
                if (genre.Value.count > 1 && !Regex.IsMatch(genre.Key.Trim(), @"^\d"))
                {
                    tsv = tsv + "\n" + genre.Key;
                    
                    foreach (var year in AllYears)
                    {
                        if(year != @"\N")
                        {
                            if (genre.Value.years.ContainsKey(year))
                                if (genre.Value.years[year] > peakCount)
                                {
                                    peakYear = year;
                                    peakCount = genre.Value.years[year];
                                }
                        }
                        if (genre.Value.years.ContainsKey(year))
                            tsv = tsv + "\t" + genre.Value.years[year];
                        else
                            tsv = tsv + "\t0";
                    }
                    Thread.Sleep(25);
                }
              //  Console.WriteLine(" Creating genre list for " + genre.Key + " (" + genre.Value.count + " titles) - Peak year: " + peakYear + "(" + peakCount + ") ");
            }
            Console.WriteLine("Creating primary genres rows");
            foreach (var genre in primarygenreList.OrderBy(x => x.Value.count))
            {
                if (genre.Value.count > 1 && !Regex.IsMatch(genre.Key.Trim(), @"^\d"))
                {
                    tsv2 = tsv2 + "\n" + genre.Key;
                    foreach (var year in AllYears)
                    {
                        if (genre.Value.years.ContainsKey(year))
                            tsv2 = tsv2 + "\t" + genre.Value.years[year];
                        else
                            tsv2 = tsv2 + "\t0";
                    }
                }
            }
            Console.WriteLine("Creating average runtime table");
            tsv3 += "\n" + "runtime";
            foreach(var year in averageRuntime)
            {
                tsv3 = tsv3 + "\t" + year.Value.total / year.Value.count;
            }
            

            Console.WriteLine("Writing file 'allgenres.tsv'");
            SaveFile(AppDomain.CurrentDomain.BaseDirectory + @"\allgenres.tsv", tsv);
            Console.WriteLine("Success!");

            Console.WriteLine("Writing file 'primarygenres.tsv'");
            SaveFile(AppDomain.CurrentDomain.BaseDirectory + @"\primarygenres.tsv", tsv2);
            Console.WriteLine("Success!");

            Console.WriteLine("Writing file 'runtimes.tsv'");
            SaveFile(AppDomain.CurrentDomain.BaseDirectory + @"\runtimes.tsv", tsv3);
            Console.WriteLine("Success!");
        }
        static void SaveFile(string path, string data)
        {
            System.IO.File.WriteAllText(path, data);
        }
        static void HackermanIntroBullshit(string[] args)
        {
            Console.WriteLine("ADVANCED IMDB DATA PROCESSOR V.1.0");
            Console.Write("Loading");
            Thread.Sleep(200);
            Console.Write("\rLoading.");
            Thread.Sleep(200);
            Console.Write("\rLoading..");
            Thread.Sleep(200);
            Console.Write("\rLoading...");
            Thread.Sleep(200);
            Console.Write("\rLoading....");
            Thread.Sleep(200);
            Console.Write("\rLoading.....");
            Thread.Sleep(200);
            Console.Write("\rLoading......");
            Thread.Sleep(200);
            Console.Write("\rLoading.......");
            Console.WriteLine("Loaded!");
            Thread.Sleep(200);
            Console.WriteLine("Opening file " + args[0]);
            Thread.Sleep(500);

            Console.WriteLine("Penetrating mainframe database");
            Thread.Sleep(300);
            Console.WriteLine("Firewall rejected proxy VPN attempt");
            Thread.Sleep(350);
            Console.Write("\rBypassing firewall.");
            Thread.Sleep(300);
            Console.Write("\rBypassing firewall..");
            Thread.Sleep(300);
            Console.Write("\rBypassing firewall...");
            Thread.Sleep(300);
            Console.Write("\rBypassing firewall....");
            Thread.Sleep(300);
            Console.Write("\rBypassing firewall.....");
            Thread.Sleep(300);
            Console.Write("\rBypassing firewall......");
            Thread.Sleep(300);
            Console.Write("\rBypassing firewall.......");

            Thread.Sleep(800);
            Console.WriteLine("Access granted to database!");

            Thread.Sleep(200);
            Console.WriteLine("Intruder detected");
            Thread.Sleep(200);
            Console.WriteLine("Intruder detected");
            Thread.Sleep(200);
            Console.WriteLine("Intruder detected");
            Thread.Sleep(200);
            Console.WriteLine("Intruder detected");
            Thread.Sleep(200);
            Console.WriteLine("Intruder detected");
            Thread.Sleep(200);
            Console.WriteLine("Intruder detected");
            Thread.Sleep(200);
            Console.WriteLine("Intruder detected");
            Thread.Sleep(500);
            Console.WriteLine("Intruder retro-hacked");
            Thread.Sleep(200);
            Console.WriteLine("Reversing access routes");
            Thread.Sleep(100);
            Console.Write("8473265243869237520368760248659254");
            Thread.Sleep(100);
            Console.Write("\r4809262859457902375428397589240575");
            Thread.Sleep(100);
            Console.Write("\r4986276859276802467892400996857544");
            Thread.Sleep(100);
            Console.Write("\r1345413513451378416348915069547254");
            Thread.Sleep(100);
            Console.Write("\r7852601395702986748903750932932489");
            Thread.Sleep(100);
            Console.WriteLine("\r5235786296395863098472843059724658");
            Console.WriteLine("Database access re-routed to mainframe server");
            Console.WriteLine("Initializing database keys");
            Thread.Sleep(300);
        }
    }
}
