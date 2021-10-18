FROM mcr.microsoft.com/dotnet/sdk:3.1-focal AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY *.csproj ./
COPY plugins/bitcoin/*.csproj ./plugins/bitcoin/
COPY plugins/erc20/*.csproj ./plugins/erc20/
COPY plugins/ethereum/*.csproj ./plugins/ethereum/
COPY plugins/ethless/*.csproj ./plugins/ethless/
RUN dotnet restore

# Copy everything else and build
COPY . ./
RUN dotnet publish plugins/bitcoin/*.csproj -c Release -o plugin-out
RUN dotnet publish plugins/erc20/*.csproj -c Release -o plugin-out
RUN dotnet publish plugins/ethereum/*.csproj -c Release -o plugin-out
RUN dotnet publish plugins/ethless/*.csproj -c Release -o plugin-out
RUN dotnet publish -c Release -o app-out

# Build runtime image
FROM mcr.microsoft.com/dotnet/sdk:3.1-focal
WORKDIR /home/Creditcoin/Gateway

COPY --from=build-env /app/app-out /home/Creditcoin/Gateway/
COPY --from=build-env /app/plugin-out /home/Creditcoin/Gateway/plugins/