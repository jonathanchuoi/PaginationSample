FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["PaginationSample/PaginationSample.csproj", "PaginationSample/"]
RUN dotnet restore "PaginationSample/PaginationSample.csproj"
COPY . .
WORKDIR "/src/PaginationSample"
RUN dotnet build "PaginationSample.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PaginationSample.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PaginationSample.dll"]
