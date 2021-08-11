#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build
WORKDIR /src
COPY ["Integral.BackendServer/Integral.BackendServer.csproj", "Integral.BackendServer/"]
COPY ["Integral.Actors/Integral.Actors.csproj", "Integral.Actors/"]
RUN dotnet restore "Integral.BackendServer/Integral.BackendServer.csproj"
COPY . .
WORKDIR "/src/Integral.BackendServer"
RUN dotnet build "Integral.BackendServer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Integral.BackendServer.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Integral.BackendServer.dll"]
