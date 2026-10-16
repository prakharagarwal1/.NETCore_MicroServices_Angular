FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY backend/services/LearningShop.CartService/LearningShop.CartService.csproj backend/services/LearningShop.CartService/
RUN dotnet restore "backend/services/LearningShop.CartService/LearningShop.CartService.csproj"
COPY backend/services/LearningShop.CartService/ backend/services/LearningShop.CartService/
WORKDIR /src/backend/services/LearningShop.CartService
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 5003
ENTRYPOINT ["dotnet", "LearningShop.CartService.dll"]