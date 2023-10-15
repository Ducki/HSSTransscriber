FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Transcriber.Browser/Transcriber.Browser.csproj", "Transcriber.Browser/"]
RUN dotnet restore "Transcriber.Browser/Transcriber.Browser.csproj"
COPY . .
WORKDIR "/src/Transcriber.Browser"
RUN dotnet build "Transcriber.Browser.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Transcriber.Browser.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Transcriber.Browser.dll"]
