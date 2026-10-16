FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY backend/services/LearningShop.ProductService/LearningShop.ProductService.csproj backend/services/LearningShop.ProductService/
RUN dotnet restore "backend/services/LearningShop.ProductService/LearningShop.ProductService.csproj"
COPY backend/services/LearningShop.ProductService/ backend/services/LearningShop.ProductService/
WORKDIR /src/backend/services/LearningShop.ProductService
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 5001
ENTRYPOINT ["dotnet", "LearningShop.ProductService.dll"]
