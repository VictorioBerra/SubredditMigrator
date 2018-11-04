using System;
using McMaster.Extensions.CommandLineUtils;
using System.Linq;
using RedditSharp;

namespace reddit_importer
{
    class Program
    {
        [Option("--targetusername")]
        public string TargetUsername { get; set; }

        [Option("--targetpassword")]
        public string TargetPassword { get; set; }

        [Option("--destusername")]
        public string DestUsername { get; set; }

        [Option("--destpassword")]
        public string DestPassword { get; set; }

        [Option]
        public bool Verbose { get; set; }

        public static int Main(string[] args)
            => CommandLineApplication.Execute<Program>(args);

        private void OnExecute()
        {

            if (string.IsNullOrEmpty(TargetUsername) ||
                string.IsNullOrEmpty(TargetPassword) ||
                string.IsNullOrEmpty(DestUsername) ||
                string.IsNullOrEmpty(DestPassword))
            {
                throw new Exception("missing required info");
            }

            Console.WriteLine($"Attempting login for {TargetUsername}");  

            var importReddit = new Reddit();
            importReddit.LogIn(TargetUsername, TargetPassword);
            var importAccount = importReddit.User;

            Console.WriteLine($"Attempting login for {DestUsername}");  

            var exportReddit = new Reddit();
            exportReddit.LogIn(DestUsername, DestPassword);

            Console.WriteLine($"Starting subreddit migration.");  

            Log(Verbose, $"Enumerating subreddit count for {TargetUsername}.");

            int totalSubscriptions = importAccount.SubscribedSubreddits.Count();
            int currentSubscription = 0;

            Log(Verbose, $"Getting names for {totalSubscriptions} subreddits");

            foreach (string subreddit in importAccount.SubscribedSubreddits.Select(x => x.Name))
            {
                Console.WriteLine($"Subscribing {exportReddit.User} to {subreddit} ({currentSubscription + 1} of {totalSubscriptions})");  

                currentSubscription++;
                
                RedditSharp.Things.Subreddit subredditToResubscribe = null;

                try
                {
                    subredditToResubscribe = exportReddit.GetSubreddit(subreddit);

                    if(subredditToResubscribe == null)
                    {
                        Console.WriteLine($"Failed to fetch {subreddit}. Skipping subscription.");  
                    }
                    else
                    {
                        subredditToResubscribe.Subscribe();
                    }

                }
                catch(Exception ex)
                {
                    Console.WriteLine($"Failed to fetch {subreddit}. Skipping subscription.");

                    Log(Verbose, ex.Message);
                }
                
                Log(Verbose, $"Successfully subscribed {exportReddit.User} to {subreddit}");
                
            }
        }

        private static void Log(bool verbose, string message){
            if(verbose)
            {
                Console.WriteLine(message);
            }
        }

    }
}
