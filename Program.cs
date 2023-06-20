using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using Octokit;
using Microsoft.Extensions.Configuration;

namespace OctokitExample
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var token = "UNKNOWN";
            var bDir = System.AppContext.BaseDirectory;
            var wDir = Path.GetFullPath(".");
            var appJson = Path.Combine(wDir,"appsettings.json");
            long fsize = new System.IO.FileInfo(appJson).Length;
            Console.WriteLine("appJson:{0} {1}",appJson,fsize.ToString());
            // Read the personal access token from the configuration file
            if (! File.Exists(appJson) || fsize == 0) {
                // Prompt user for GitHub token
                Console.Write("Enter Github Token:");
                string? inputStr = Console.ReadLine();
                if (! string.IsNullOrEmpty(inputStr)) {
                    token = inputStr;
                }
            } else {
                try {
                    var config = new ConfigurationBuilder()
                        .AddJsonFile(appJson)
                        .Build();
                    if (! string.IsNullOrEmpty(config["GitHubToken"])) {
                        token = config["GitHubToken"];
                    } else {
                        // Prompt user for GitHub token
                        Console.Write("Enter Github Token:");
                        string? inputStr = Console.ReadLine();
                        if (! string.IsNullOrEmpty(inputStr)) {
                            token = inputStr;
                        }
                    }
                } catch (Exception e) {
                    Console.WriteLine("***WHAT*** Exception:{0}", e.Message);
                    // Prompt user for GitHub token
                    Console.Write("Enter Github Token:");
                    string? inputStr = Console.ReadLine();
                    if (! string.IsNullOrEmpty(inputStr)) {
                        token = inputStr;
                    }
                }
            }
            if (token == "UNKNOWN") {
                Console.WriteLine("ERROR: Missing GitHub Token!");
                Environment.Exit(0);
            }
            
            var github = new GitHubClient(new ProductHeaderValue("OctokitApp"));
            github.Credentials = new Credentials(token);

            var repos = await GetGithubRepositoryAsync(github);
            foreach (var repo in repos)
            {
                Console.WriteLine(repo.FullName);
            }
        }

        public static async Task<List<Repository>> GetGithubRepositoryAsync(GitHubClient github)
        {
            var repos = await github.Repository.GetAllForCurrent();
            return repos.ToList();
        }
    }
}
