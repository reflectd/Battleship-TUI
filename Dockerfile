# App build environment
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
RUN apk add --no-cache clang
WORKDIR /App
COPY src/ .
RUN dotnet publish -c Release -p:SelfContained=true -p:PublishAot=true -o out

# Runtime image
FROM alpine
RUN apk add --no-cache ncurses
WORKDIR /App
COPY --from=build /App/out/ .
ENTRYPOINT ["./Battleship"]