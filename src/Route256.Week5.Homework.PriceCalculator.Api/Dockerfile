FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
ENV ASPNETCORE_ENVIRONMENT=Development
WORKDIR /src
COPY ["src/Route256.Week5.Homework.PriceCalculator.Api/Route256.Week5.Homework.PriceCalculator.Api.csproj", "src/Route256.Week5.Homework.PriceCalculator.Api/"]
COPY ["src/Route256.Week5.Homework.PriceCalculator.Bll/Route256.Week5.Homework.PriceCalculator.Bll.csproj", "src/Route256.Week5.Homework.PriceCalculator.Bll/"]
COPY ["src/Route256.Week5.Homework.PriceCalculator.Dal/Route256.Week5.Homework.PriceCalculator.Dal.csproj", "src/Route256.Week5.Homework.PriceCalculator.Dal/"]
RUN dotnet restore "src/Route256.Week5.Homework.PriceCalculator.Api/Route256.Week5.Homework.PriceCalculator.Api.csproj"
COPY . .
WORKDIR "/src/src/Route256.Week5.Homework.PriceCalculator.Api"
RUN dotnet build "Route256.Week5.Homework.PriceCalculator.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Route256.Week5.Homework.PriceCalculator.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Route256.Week5.Homework.PriceCalculator.Api.dll"]
