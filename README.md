# Instadicey

Instadicey is a full-stack Instagram clone built using React for the frontend and ASP.NET Core for the backend. It supports user authentication, posting images, liking posts, and more. The app can be run locally or containerized using Docker Compose.

> ⚠️ **Note:** The frontend was developed ~4 years ago when I was still learning web development. While it's still fully functional, It’s outdated, and not reflective of modern best practices, and definitely not my best work. I’m currently a backend developer, so the frontend for this project is just to showcase the backend. Which on the other hand, is solid.

## 🧱 Tech Stack

- **Frontend**: React (JavaScript)
- **Backend**: ASP.NET Core Web API
- **Database**: PostgreSQL
- **Authentication**: JWT (JSON Web Token)
- **Containerization**: Docker & Docker Compose

## 📷 Pictures
![Login Page](https://user-images.githubusercontent.com/85850551/139522554-fc78e1d9-8b6e-4916-a264-8993ed98a957.png)

![Home Page](https://user-images.githubusercontent.com/85850551/139522564-e3e66a57-4297-4afa-8ad7-ef28bbc2a01d.png)

![Profile Page](https://user-images.githubusercontent.com/85850551/139522568-95e6d930-f850-456f-bef6-57a62ffc3ac9.png)

![Post Page](https://user-images.githubusercontent.com/85850551/139522586-c1a8ca41-ffd6-42ff-a6f5-efb1f7d42440.png)

## 🚀 Getting Started
### Prerequisites
- [.NET SDK](https://dotnet.microsoft.com/en-us/download)
- [PostgreSQL](https://www.postgresql.org/download/) (Or you can also connect to an online instance)
- [Node.js](https://nodejs.org/)
#### Optional
- [Docker](https://www.docker.com/)
- [Docker Compose](https://docs.docker.com/compose/)

## 🔧 Configuration
Before running the backend, you **must fill in the required values** in your `appsettings.json` or pass them via environment variables (especially when using Docker).

Here’s a sample of the required structure:

```json
{
  "ConnectionStrings:Main": "",
  "Authentication:Schemes:Bearer:SigningKeys": [
	{
		"Issuer": "",
		"Value": ""
	}
  ],
  "SMTP": {
    "Key": ""
  },
  "AES": {
    "Key": "",
    "IV": ""
  },
  "UseS3Cloud": true || false,
  "UseEmailVerification":  true,
  "S3": {
    "AccessKey": "",
    "SecretKey": "",
    "ServiceURL": ""
  }
}
```
### 🔑 Explanation of each field
- **ConnectionStrings:Main**: Your database connection string for PostgreSQL. (Could be for an online instance or a local one)
- **Authentication:Schemes:Bearer:SigningKeys**: Used for validating JWTs. Issuer can be anything, and Value is your Base64 encoded secret symmetric signing key, which can be an encoded random string of at least 32 bytes (or 32 encoded ASCII characters)
- **SMTP:Key**: API key for sending emails via SMTP (used for verification, password reset, etc). Can be obtained by [following this help article](https://support.google.com/mail/answer/185833)
- **AES:Key**: Key used for AES encryption of sensitive data, which can be a  Base64 encoded random string representing least 16 bytes (or 16 encoded ASCII characters)
- **AES:IV**: Key made up of a Base64 encoded string representing exactly 16 bytes (or 16 encoded ASCII characters)
- **UseS3Cloud**: Defines where files are stored
- **UseEmailVerification**: If true, you'll have to provide the SMTP key, and users will receive verification by email. If false, the verification link for each user will be printed in the console.
- **S3:***: Credentials for an S3-compatible cloud storage (For when UseS3Cloud is true)

## 💻 Run Locally (No Docker)
### 1. Clone the repo

```bash
git clone https://github.com/shadowspedicey/instadicey-instagram-clone.git
cd instadicey-instagram-clone
```
### 2. Run Backend
```bash
cd backend/src/InstagramClone
dotnet build
dotnet ef database update
dotnet run
```
### 3. Run Frontend
```bash
cd ../../../frontend
npm install
npm start
```

## 🐳 Running with Docker Compose
### 1. Provide the required configuration values in the `api.env` file
```
ConnectionStrings__Main=
Authentication__Schemes__Bearer__SigningKeys__0__Issuer=
Authentication__Schemes__Bearer__SigningKeys__0__Value=
SendGrid__Key=
AES__Key=
AES__IV=
S3__AccessKey=
S3__SecretKey=
S3__ServiceURL=
```
### 2. Run the full stack containers
```bash
docker-compose up
```
This will:
- Build the frontend and backend containers and run them
- This docker compose project DOESN'T include the PostgreSQL instance (You'll have to install it on the host machine and connect. Check out [this post](https://forums.docker.com/t/how-to-reach-localhost-on-host-from-docker-container/113321) about how to get it done)

## 🗄️ Storage Options (Local or Cloud)
Instadicey supports both local storage and cloud storage (S3-compatible) for uploaded images.

You can choose which one to use by setting the following in appsettings.json:
```json
"AppDataFolderName": "" // Default is App_Data,
"UseS3Cloud": true || false
```
### 🗄️ Local
- Saves files to a local folder in the app directory defined by the `AppDataFolderName` entry
- No credentials needed
### ☁️ Cloud
- Uses Amazon S3 or any S3-compatible service (Like [Supabase](https://supabase.com/)).
- Must provide the following:
```json
  "S3": {
    "AccessKey": "your-access-key",
    "SecretKey": "your-secret-key",
    "ServiceURL": "https://s3.your-provider.com"
  }
```

## 🧑‍🚀 Guest Mode
Instadicey includes a **Guest Mode**, allowing users to explore the app without signing up. This is useful for demos or letting users try the app before registering.

A single guest account is used for this purpose and **gets automatically reset at a configurable interval**.

### 🔁 Guest Account Reset Interval

You can change how often the guest account resets by modifying the `GuestResetIntervalInMinutes` property in `appsettings.json`:

```json
"GuestResetIntervalInMinutes": 5,
```

## 🚀 Live Site
You can check out the live version of this app through here:



[<strong style="font-size: 2.5rem">🔗Instadicey</strong>](https://shadowspedicey.github.io/instadicey-instagram-clone/#/)
> **Note:**  
> • The frontend is hosted on [GitHub Pages](https://pages.github.com/)  
> • The backend is hosted on [Render](https://render.com), so it may take a few seconds to respond if inactive.  