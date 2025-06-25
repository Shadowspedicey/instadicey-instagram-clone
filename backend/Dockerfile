FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

WORKDIR /src

COPY ./src/InstagramClone/*.csproj .

RUN dotnet restore

COPY ./src/InstagramClone/. .

RUN dotnet build -c Release -o /app --no-restore



FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime

WORKDIR /app

COPY --from=build /app .

ENV ASPNETCORE_URLS="http://0.0.0.0:5000"

EXPOSE 5000

ENTRYPOINT [ "dotnet", "InstagramClone.dll" ]
