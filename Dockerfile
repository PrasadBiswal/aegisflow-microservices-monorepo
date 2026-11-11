# syntax=docker/dockerfile:1

# One Dockerfile for all 10 services. Build one service with:
#   docker build --build-arg SERVICE_NAME=orders-api -t aegisflow/orders-api .

# ---------- Build stage: SDK 10 builds the net8.0 service (see global.json) ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG SERVICE_NAME
RUN test -n "$SERVICE_NAME" || (echo "ERROR: pass --build-arg SERVICE_NAME=<folder name under services/>" >&2 && exit 1)
WORKDIR /src

# 1) Restore packages. This layer stays cached until a project file or package version changes.
#    The NuGet cache mount is shared by all 10 service builds, so each package downloads only once.
COPY global.json nuget.config Directory.Build.props Directory.Packages.props ./
COPY libs/AegisFlow.ServiceDefaults/AegisFlow.ServiceDefaults.csproj libs/AegisFlow.ServiceDefaults/
COPY services/${SERVICE_NAME}/${SERVICE_NAME}.csproj services/${SERVICE_NAME}/
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages,sharing=locked \
    dotnet restore services/${SERVICE_NAME}/${SERVICE_NAME}.csproj

# 2) Copy the source code and publish. (No --no-restore: still works if the cache was cleared.)
COPY libs/ libs/
COPY services/${SERVICE_NAME}/ services/${SERVICE_NAME}/
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages,sharing=locked \
    dotnet publish services/${SERVICE_NAME}/${SERVICE_NAME}.csproj \
      --configuration Release --output /app/publish -p:UseAppHost=false

# ---------- Runtime stage: small image with only the ASP.NET Core 8 runtime ----------
FROM mcr.microsoft.com/dotnet/aspnet:11.0 AS runtime
ARG SERVICE_NAME
ENV SERVICE_NAME=${SERVICE_NAME}
LABEL org.opencontainers.image.title="${SERVICE_NAME}" \
      org.opencontainers.image.source="https://github.com/PrasadBiswal/aegisflow-microservices-monorepo"
WORKDIR /app
COPY --from=build /app/publish .

# Run as the image's built-in non-root user.
USER $APP_UID
EXPOSE 8080

# "exec" makes dotnet process 1, so it receives the stop signal from Docker/Kubernetes
# and shuts down gracefully. "$@" passes extra arguments through, e.g. "migrate".
ENTRYPOINT ["sh", "-c", "exec dotnet \"$SERVICE_NAME.dll\" \"$@\"", "--"]
