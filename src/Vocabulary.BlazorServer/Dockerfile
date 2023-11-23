#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["src/Vocabulary.BlazorServer/Vocabulary.BlazorServer.csproj", "src/Vocabulary.BlazorServer/"]
COPY ["src/Vocabulary.Adapters/Vocabulary.Adapters.csproj", "src/Vocabulary.Adapters/"]
COPY ["src/Vocabulary/Vocabulary.csproj", "src/Vocabulary/"]
RUN dotnet restore "src/Vocabulary.BlazorServer/Vocabulary.BlazorServer.csproj"
COPY . .
WORKDIR "/src/src/Vocabulary.BlazorServer"
RUN dotnet build "Vocabulary.BlazorServer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Vocabulary.BlazorServer.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Vocabulary.BlazorServer.dll"]