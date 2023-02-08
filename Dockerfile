#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["NewTicketApi/NewTicketApi.csproj", "NewTicketApi/"]
COPY ["AWS.PM.Entity/AWS.PM.Entity.csproj", "AWS.PM.Entity/"]
COPY ["AWS.PM.Interfaces/AWS.PM.Interfaces.csproj", "AWS.PM.Interfaces/"]
COPY ["AWS.PM.Service/AWS.PM.Service.csproj", "AWS.PM.Service/"]
RUN dotnet restore "NewTicketApi/NewTicketApi.csproj"
COPY . .
WORKDIR "/src/NewTicketApi"
RUN dotnet build "NewTicketApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "NewTicketApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NewTicketApi.dll"]