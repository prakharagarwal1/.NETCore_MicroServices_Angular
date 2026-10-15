FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY backend/services/LearningShop.OrderService/LearningShop.OrderService.csproj backend/services/LearningShop.OrderService/
RUN dotnet restore "backend/services/LearningShop.OrderService/LearningShop.OrderService.csproj"
COPY backend/services/LearningShop.OrderService/ backend/services/LearningShop.OrderService/
WORKDIR /src/backend/services/LearningShop.OrderService
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 5102
ENTRYPOINT ["dotnet", "LearningShop.OrderService.dll"]