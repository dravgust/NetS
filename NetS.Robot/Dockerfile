#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/runtime:3.1-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["NetS.Robot/NetS.Robot.csproj", "NetS.Robot/"]
RUN dotnet restore "NetS.Robot/NetS.Robot.csproj"
COPY . .
WORKDIR "/src/NetS.Robot"
RUN dotnet build "NetS.Robot.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "NetS.Robot.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NetS.Robot.dll"]