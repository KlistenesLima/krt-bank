FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "src/Services/KRT.Payments/KRT.Payments.Api/KRT.Payments.Api.csproj"
RUN dotnet publish "src/Services/KRT.Payments/KRT.Payments.Api/KRT.Payments.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:80
ENV QuestPDF__Settings__License=Community
ENTRYPOINT ["dotnet", "KRT.Payments.Api.dll"]