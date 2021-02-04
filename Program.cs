using System;
using McMaster.Extensions.CommandLineUtils;
using System.Linq;
using RedditSharp;
using RedditSharp.Things;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace reddit_importer
{
    class Program
    {
        [Required]
        [Option("--targetusername", "The account that will be migrated. This account has your desired subs and saved things.", CommandOptionType.SingleValue)]
        public string TargetUsername { get; set; }

        [Required]
        [Option("--targetpassword")]
        public string TargetPassword { get; set; }

        [Required]
        [Option("--destusername", "The account that will end up with the target accounts subs and saved things.", CommandOptionType.SingleValue)]
        public string DestUsername { get; set; }

        [Required]
        [Option("--destpassword")]
        public string DestPassword { get; set; }

        [Required]
        [Option("--clientid")]
        public string clientid { get; set; }

        [Required]
        [Option("--clientsecret")]
        public string clientsecret { get; set; }

        [Option("--redirecturi")]
        public string redirecturi { get; set; } = "http://localhost/";

        [Option]
        public bool Verbose { get; set; }

        [Option("--migratesubs", "Migrate subreddit subscriptions to new account.", CommandOptionType.NoValue)]
        public bool MigrateSubs { get; set; }

        [Option("--deleteallpostsandcomments", "Delete all posts and comments on the target account.", CommandOptionType.NoValue)]
        public bool DeleteAllPostsAndComments { get; set; }

        [Option("--migratesaved", "Migrate saved posts and comments to new account.", CommandOptionType.NoValue)]
        public bool MigrateSaved { get; set; }

        public static int Main(string[] args)
            => CommandLineApplication.Execute<Program>(args);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Called by CommandLineApplication")]
        private async System.Threading.Tasks.Task OnExecuteAsync()
        {
            if(!this.MigrateSubs && !this.DeleteAllPostsAndComments && !this.MigrateSaved)
            {
                Console.WriteLine("Please specify a flag. See --help.");
                return;
            }

            if(this.MigrateSubs)
            {
                Console.WriteLine("I will migrate your subreddits.");
            }

            if (this.MigrateSaved)
            {
                Console.WriteLine("I will migrate your saved items.");
            }

            if (this.DeleteAllPostsAndComments)
            {
                Console.WriteLine("I will delete all your comments and posts.");
            }

            var result = Prompt.GetYesNo("Do you want to proceed?", false);
            if(!result)
            {
                return;
            }

            Console.WriteLine($"Attempting login for {TargetUsername}");
            var targetRedditAgent = new BotWebAgent(TargetUsername, TargetPassword, clientid, clientsecret, redirecturi);
            var targetReddit = new Reddit(targetRedditAgent, initUser: true);
            Console.WriteLine($"Success.");

            Console.WriteLine($"Attempting login for {DestUsername}");
            var destinationRedditAgent = new BotWebAgent(DestUsername, DestPassword, clientid, clientsecret, redirecturi);
            var destinationReddit = new Reddit(destinationRedditAgent, initUser: true);
            Console.WriteLine($"Success.");

            if (MigrateSubs)
            {
                Console.WriteLine($"Starting subreddit migration.");

                await targetReddit.User.GetSubscribedSubreddits().ForEachAsync(async (subreddit, index) => {

                    try
                    {
                        Console.WriteLine($"({index}) - Fetching {subreddit.Name}.");
                        var subredditToResubscribe = await destinationReddit.GetSubredditAsync(subreddit.Name, validateName: false);
                        await subredditToResubscribe.SubscribeAsync();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to fetch {subreddit}. Skipping subscription.");
                        Log(Verbose, ex.Message);
                    }
                });
            }
            else
            {
                Console.WriteLine("Skipping subreddit migration. Use --migratesubs");
            }

            if(DeleteAllPostsAndComments)
            {
                await targetReddit.User.GetComments(limit: 100).ForEachAsync(async (comment, index) =>
                {
                    try
                    {
                        Console.WriteLine($"({index}) - Deleting comment {comment.Id}.");
                        await comment.DelAsync();
                    } catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to fetch {comment.Id}. Skipping comment.");
                        Log(Verbose, ex.Message);
                    }
                });
                await targetReddit.User.GetPosts(limit: 100).ForEachAsync(async (post, index) =>
                {
                    try
                    {
                        Console.WriteLine($"({index}) - Deleting post {post.Id}.");
                        await post.DelAsync();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to fetch {post.Id}. Skipping post.");
                        Log(Verbose, ex.Message);
                    }
                });
            }
            else
            {
                Console.WriteLine("Skipping delete all posts and comments. Use --deleteallpostsandcomments");
            }

            if (MigrateSaved)
            {
                Console.WriteLine("Migrating saved.");

                await targetReddit.User.GetSaved().ForEachAsync(async (thing, index) =>
                {
                    try
                    {
                        Console.WriteLine($"({index}) - Getting saved thing {thing.Id}.");
                        var savedThingDest = await destinationReddit.GetThingByFullnameAsync(thing.FullName);
                        if (savedThingDest is not null)
                        {
                            switch (savedThingDest)
                            {
                                case Comment:
                                    await ((Comment)savedThingDest).SaveAsync();
                                    break;
                                case Post:
                                    await ((Post)savedThingDest).SaveAsync();
                                    break;
                                default:
                                    throw new NotImplementedException("Unsupported saved thing.");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to fetch {thing.Id}. Skipping comment.");
                        Log(Verbose, ex.Message);
                    }
                });
            }
            else
            {
                Console.WriteLine("Skipping migrating saved posts. Use --migratesaved");
            }

            Console.WriteLine("Done!");

        }

        private static void Log(bool verbose, string message){
            if(verbose)
            {
                Console.WriteLine(message);
            }
        }

    }
}
