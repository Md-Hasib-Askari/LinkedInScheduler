# LinkedIn Post Scheduler — drop-in module for your .NET app

Free, official-API-based scheduling. No browser extension, no ToS risk.

## 1. Create the LinkedIn app (one-time, ~5 min)

1. Go to <https://www.linkedin.com/developers/apps> → **Create app**.
2. Fill in the name/logo. You'll be asked to link a **Company Page** — this is
   required by LinkedIn even for posting to your *personal* profile. If you
   don't have one, create a placeholder page first, it costs nothing.
3. Under the **Products** tab, add:
   - **Sign In with LinkedIn using OpenID Connect**
   - **Share on LinkedIn**
   Both are self-serve — no waiting on LinkedIn's review team.
4. Under **Auth**, add a redirect URL matching `RedirectUri` in your config,
   e.g. `https://localhost:5001/linkedin/callback`.
5. Copy the **Client ID** and **Client Secret** from the Auth tab.

## 2. Wire up the project

NuGet packages needed:

```
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add package Microsoft.EntityFrameworkCore.Design
```

(Skip these if you're plugging `ScheduledPost`/`LinkedInToken` straight into a
Postgres DbContext you already have.)

`appsettings.json`:

```json
{
  "ConnectionStrings": {
    "SchedulerDb": "Host=localhost;Port=5432;Database=linkedin_scheduler;Username=postgres;Password=YOUR_PASSWORD"
  },
  "LinkedIn": {
    "ClientId": "YOUR_CLIENT_ID",
    "ClientSecret": "YOUR_CLIENT_SECRET",
    "RedirectUri": "https://localhost:5001/linkedin/callback",
    "PollIntervalSeconds": 60
  }
}
```

Better: keep the DB password and `ClientSecret` out of appsettings and use
`dotnet user-secrets set "ConnectionStrings:SchedulerDb" "..."` /
`dotnet user-secrets set "LinkedIn:ClientSecret" "..."` locally.

If you want a free hosted Postgres instead of a local install, Supabase and
Neon both have an always-free tier and give you a connection string in the
same format.

`Program.cs` additions:

```csharp
builder.Services.Configure<LinkedInOptions>(builder.Configuration.GetSection("LinkedIn"));
builder.Services.AddDbContext<SchedulerDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("SchedulerDb")));

builder.Services.AddHttpClient<LinkedInAuthService>();
builder.Services.AddHttpClient<LinkedInPostingService>();
builder.Services.AddHostedService<PostSchedulerBackgroundService>();

builder.Services.AddControllers(); // if not already present
// ...
app.MapControllers(); // if not already present
```

Create the DB (or add a migration if you're merging into an existing context):

```
dotnet ef migrations add InitScheduler
dotnet ef database update
```

This creates the tables in your Postgres database via the connection string
above — make sure the database itself (`linkedin_scheduler` or whatever you
name it) already exists before running it.

## 3. Connect your account (one-time)

Run the app, visit `https://localhost:5001/linkedin/login`, approve access.
That's it — the refresh token is good for ~365 days, so you won't see this
screen again unless the tool sits unused for a year.

## 4. Queue posts

Insert rows into `ScheduledPosts` with `Content` and `ScheduledTimeUtc`
(UTC!). Since your posts already live in your existing localhost app, the
simplest bridge is to add a small endpoint or a "Schedule" button there that
inserts a row here — or just add `ScheduledTimeUtc`/`Status` columns directly
to your existing posts table and point `SchedulerDbContext` at it instead of
using a separate `ScheduledPost` table.

## 5. Keep the process running

LinkedIn's API has no built-in scheduling — this app's background loop *is*
the scheduler, so it needs to be alive when a post is due. Free options:

- **Simplest**: just leave your localhost app running on your machine at the
  times you've scheduled posts for.
- **Set-and-forget, still free**: deploy to an always-free tier — Oracle
  Cloud's free VM tier or Fly.io's free allowance both run a small .NET app
  fine, or publish as a Windows/Linux service with Task Scheduler / systemd
  timer restarting it if it crashes.

## Good to know

- Rate limit is roughly 100 calls/day per member — far more than you'll use.
- There's no "edit post" endpoint. Fixing a typo means deleting the post and
  publishing a new one.
- Access tokens expire every 60 days; this module auto-refreshes using the
  365-day refresh token, so you don't need to babysit it.
