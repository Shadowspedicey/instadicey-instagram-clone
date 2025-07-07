# Instadicey (Backend)
This is the backend section of the fullstack Instagram clone project (Instadicey)

Made with ASP.NET Core and Microsoft SQL Server

# ERD
The entity relationship diagram of the app, which shows an overview of the entities that compose the app, and how they can interact with eachother (relationships), in addition to any attributes each entity may have.

This does not accurately represent the technical side of the database fully, but it's meant to show how the planning of the app's structure.

![ERD](ERD.png "The ERD")

This ERD was made using [ERDPlus](https://erdplus.com/).

# Features
✅ Postgres - Database

✅ JWT tokens - Generates and validates short-lasting JWTs to use for authentication

✅ Refresh tokens - Uses long-lasting refresh tokens to allow for a smoother user experience while ensuring security

✅ ASP.NET Identity - Account management (authorization, authentication)

✅ Email verification - Used Gmail SMTP with Identity to send verification emails (for sign up)

✅ Problem Details - Error responses follow problem details RFC 7807 specifications

✅ Testing - Used xUnit for unit tests, and Sqlite (in-memory) for integration tests

✅ AES Encryption - Encryptes file paths so that the paths are not exposed when being retreived

✅ SignalR - Real-time messaging

✅ Guest Account - Users can use a guest account to use the application without creating an account (NOTE: This guest account is reset every interval, which has a default value of 5 minutes)