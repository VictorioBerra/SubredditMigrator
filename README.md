# SubredditMigrator

Migrate subreddits and saved posts between 2 accounts.

# Install

Grab the exe from releases.

# Building From Sauce

Should support cross-plat since its net5.0. Clone, change .csproj runtime/output, publish, enjoy.

## Set Up

Go here 👉 https://www.reddit.com/prefs/apps clickthe buttom at the bottom, should say "create an app" or "create another app"

**Name** can be whatever, be sure to select **script**, for **redirect uri** use `http://localhost/`

Grab your new Client ID, and Client Secret.

## Usage

Compile and run with these arguments:

```
--targetusername <targetusername>
--targetpassword <targetpassword>
--destusername <destusername>
--destpassword <destpassword>
--clientid <clientid>
--clientsecret <clientsecret>
--migratesubs
--migratesaved
--deleteallpostsandcomments
```

Sample command:

`.\reddit-importer.exe --targetusername "Account1" --targetpassword "Account1Password" --destusername "Account2" --destpassword "Account2Password" --clientid "MYCLIENTID" --clientsecret "SfXOo9YUEdwVu6fIm6BEOt6okVFx6A" --migratesubs --migratesaved`


Saved items only include posts and comments for now.

## TODO

- CICD matrix cross-plat release