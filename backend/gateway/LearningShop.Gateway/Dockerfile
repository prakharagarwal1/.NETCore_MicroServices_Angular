FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY backend/gateway/LearningShop.Gateway/LearningShop.Gateway.csproj backend/gateway/LearningShop.Gateway/
RUN dotnet restore "backend/gateway/LearningShop.Gateway/LearningShop.Gateway.csproj"
COPY backend/gateway/LearningShop.Gateway/ backend/gateway/LearningShop.Gateway/
WORKDIR /src/backend/gateway/LearningShop.Gateway
RUN dotnet publish -c Release -o /app/publish
COPY backend/gateway/LearningShop.Gateway/appsettings.Production.json /app/publish/appsettings.Production.json

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 5000
ENTRYPOINT ["dotnet", "LearningShop.Gateway.dll"]
