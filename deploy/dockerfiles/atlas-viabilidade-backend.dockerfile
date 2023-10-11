#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

# Imagem para a API
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Imagem com SDK para fazer o build da API
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["src/Viabilidade.API/Viabilidade.API.csproj", "src/Viabilidade.API/"]
COPY ["src/Viabilidade.Application/Viabilidade.Application.csproj", "src/Viabilidade.Application/"]
COPY ["src/Viabilidade.Domain/Viabilidade.Domain.csproj", "src/Viabilidade.Domain/"]
COPY ["src/Viabilidade.Infrastructure/Viabilidade.Infrastructure.csproj", "src/Viabilidade.Infrastructure/"]
COPY ["src/Viabilidade.Service/Viabilidade.Service.csproj", "src/Viabilidade.Service/"]
RUN dotnet restore "src/Viabilidade.API/Viabilidade.API.csproj"
COPY . .
WORKDIR "/src/src/Viabilidade.API"
RUN dotnet build "Viabilidade.API.csproj" -c Release -o /app/build

# Instalando dotnet tools para diagnosticos
RUN dotnet tool install --tool-path /tools dotnet-counters
RUN dotnet tool install --tool-path /tools dotnet-dump

# Gera pacote de publicacao
FROM build AS publish
RUN dotnet publish "Viabilidade.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final

# Copiando ferramentas para a imagem
WORKDIR /tools
COPY --from=build /tools .

# Copiando build para a imagem
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Viabilidade.API.dll"]