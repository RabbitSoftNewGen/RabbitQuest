# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /solution

# Copy solution file and all project files first
# Copy solution file and all project files first
COPY ["RabbitQuestAPI/RabbitQuestAPI.csproj", "RabbitQuestAPI/"]
COPY ["RabbitQuestAPI.Application/RabbitQuestAPI.Application.csproj", "RabbitQuestAPI.Application/"]
COPY ["RabbitQuestAPI.Domain/RabbitQuestAPI.Domain.csproj", "RabbitQuestAPI.Domain/"]
COPY ["RabbitQuestAPI.Infrastructure/RabbitQuestAPI.Infrastructure.csproj", "RabbitQuestAPI.Infrastructure/"]
# Add other project references as needed

# Restore dependencies
RUN dotnet restore "RabbitQuestAPI/RabbitQuestAPI.csproj"

# Copy everything else
COPY . .
WORKDIR "/solution/RabbitQuestAPI"

# Build and publish
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Create non-root user
ENV APP_UID=1654
RUN groupadd --gid=$APP_UID app 2>/dev/null || true && \
    useradd -l --uid=$APP_UID --gid=$APP_UID --create-home app 2>/dev/null || true

# Set up directories with correct permissions
RUN mkdir -p /app/data/uploads/avatars \
    && chown -R app:app /app

# Switch to non-root user
USER app

# Copy published app
COPY --from=build --chown=app:app /app/publish .

# Configure web server
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
EXPOSE 8081

ENTRYPOINT ["dotnet", "RabbitQuestAPI.dll"]