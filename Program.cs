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
            string orgName = "earnhardt3rd-forks";
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
            StringComparison comp = StringComparison.OrdinalIgnoreCase;
            foreach (var repo in repos)
            {
                if (repo.FullName.Contains("skills",StringComparison.OrdinalIgnoreCase)) {
                    Console.WriteLine("  SKILLS:{0}",repo.FullName);
                    Console.WriteLine("  HURL:{0}",repo.HtmlUrl);
                    string sOrg = "earnhardt3rd-github";
                    string surl = repo.FullName;
                    bool chk = repo.FullName.Contains(sOrg);
                    Console.WriteLine("  BOOL:{0}",chk.ToString());
                    if (chk == false) {
                        await TransferRepoToOrg(github, repo, sOrg);
                    } else {
                        Console.WriteLine("  ** Repo:{0} ALREADY TRANSFERRED TO ORG:{1}!!!",repo.FullName,sOrg);
                    }
                    continue;
                }
                if (repo.Fork)
                {
                    Console.WriteLine("  FORKED:{0}",repo.FullName);
                    Console.WriteLine("  Owner:{0}",repo.Owner);
                    Console.WriteLine("  HtmlUrl:{0}",repo.HtmlUrl);
                    
                    string url = repo.HtmlUrl;
                    if (url.Contains(orgName,comp)) {
                        Console.WriteLine("  ** Repo:{0} ALREADY TRANSFERRED TO ORG:{1}!!!",repo.FullName,orgName);
                    } else {
                        await TransferRepoToOrg(github, repo, orgName);
                        //Environment.Exit(0);
                    }
                } else {
                    Console.WriteLine("  --NOT FORKED:{0}",repo.FullName);
                }
            }
        }

        public static async Task<List<Repository>> GetGithubRepositoryAsync(GitHubClient github)
        {
            var repos = await github.Repository.GetAllForCurrent();
            return repos.ToList();
        }
        public static async Task TransferRepoToOrg(GitHubClient github, Repository repo, string orgName)
        {
            Console.WriteLine("  --Transferring Repo: {0} --> To Org:{1}",repo.FullName,orgName);
            var transfer = new RepositoryTransfer(orgName);
            await github.Repository.Transfer(repo.Id, transfer);
        }
    }
}
