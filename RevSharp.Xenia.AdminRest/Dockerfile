FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["RevSharp.Xenia.AdminRest/RevSharp.Xenia.AdminRest.csproj", "RevSharp.Xenia.AdminRest/"]
RUN dotnet restore "RevSharp.Xenia.AdminRest/RevSharp.Xenia.AdminRest.csproj"
COPY . .
WORKDIR "/src/RevSharp.Xenia.AdminRest"
RUN dotnet build "RevSharp.Xenia.AdminRest.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "RevSharp.Xenia.AdminRest.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RevSharp.Xenia.AdminRest.dll"]
