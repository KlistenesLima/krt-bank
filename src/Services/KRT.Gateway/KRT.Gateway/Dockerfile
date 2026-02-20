FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "src/Services/KRT.Gateway/KRT.Gateway/KRT.Gateway.csproj"
RUN dotnet publish "src/Services/KRT.Gateway/KRT.Gateway/KRT.Gateway.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:80
ENV QuestPDF__Settings__License=Community
ENTRYPOINT ["dotnet", "KRT.Gateway.dll"]