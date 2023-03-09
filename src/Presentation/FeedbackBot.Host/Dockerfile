FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env

COPY "FeedbackBot.sln" "FeedbackBot.sln"

WORKDIR /src
COPY ["src/Presentation/FeedbackBot.Host/FeedbackBot.Host.csproj", "Presentation/FeedbackBot.Host/"]
COPY ["src/Core/FeedbackBot.Application/FeedbackBot.Application.csproj", "Core/FeedbackBot.Application/"]
COPY ["src/Infrastructure/FeedbackBot.Persistence/FeedbackBot.Persistence.csproj", "Infrastructure/FeedbackBot.Persistence/"]
COPY ["src/Core/FeedbackBot.Domain/FeedbackBot.Domain.csproj", "Core/FeedbackBot.Domain/"]

RUN dotnet restore "../FeedbackBot.sln"

COPY src .
WORKDIR /src/Presentation/FeedbackBot.Host
RUN dotnet publish "FeedbackBot.Host.csproj" -c Release -o /publish

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS runtime
WORKDIR /publish
COPY --from=build-env /publish .
EXPOSE 80
EXPOSE 443
ENTRYPOINT ["dotnet", "FeedbackBot.Host.dll"]
