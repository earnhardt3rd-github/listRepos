using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Octokit;

namespace OctokitExample
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Prompt user for GitHub token
            Console.Write("Enter Github Token:");
            var token = Console.ReadLine();

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
