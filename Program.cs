using System;
using Microsoft.Extensions.CommandLineUtils;
using System.Linq;
using RedditSharp;

namespace reddit_importer
{
    class Program
    {
        static void Main(string[] args)
        {
            var app = new CommandLineApplication();
            app.Name = "RedditImporter";
            app.Description = "Import subreddits from another account.";

            var targetusername = app.Option("--targetusername <targetusername>",
                                    "Username of he target.",
                                    CommandOptionType.SingleValue);

            var targetpassword = app.Option("--targetpassword <targetpassword>",
                                    "Password of he target.",
                                    CommandOptionType.SingleValue);

            var destusername = app.Option("--destusername <destusername>",
                                    "Username of he destination account.",
                                    CommandOptionType.SingleValue);

            var destpassword = app.Option("--destpassword <destpassword>",
                                    "Password of the destination account.",
                                    CommandOptionType.SingleValue);

            app.OnExecute(() =>
            {

                if (!targetusername.HasValue() || !targetpassword.HasValue() || !destusername.HasValue() || !destpassword.HasValue())
                {
                    throw new Exception("missing required info");
                }

                var importReddit = new Reddit();
                importReddit.LogIn(targetusername.Value(), targetpassword.Value());
                var importAccount = importReddit.User;

                var exportReddit = new Reddit();
                exportReddit.LogIn(destusername.Value(), destpassword.Value());

                int totalSubscriptions = importAccount.SubscribedSubreddits.Count();
                int currentSubscription = 0;

                foreach (string subreddit in importAccount.SubscribedSubreddits.Select(s => s.Name))
                {
                    currentSubscription++;
                    exportReddit.GetSubreddit(subreddit).Subscribe();
                    Console.WriteLine($"Subscribed {exportReddit.User} to {subreddit} ({currentSubscription} of {totalSubscriptions})");  
                }

                return 0;
            });


            app.Execute(args);
        }
    }
}
