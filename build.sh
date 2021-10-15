#!/bin/bash
dotnet build --configuration Release
rm -rf ccgateway/bin/Release/netcoreapp3.1/plugins
mkdir ccgateway/bin/Release/netcoreapp3.1/plugins
cp ccgateway/plugins/*.dll ccgateway/bin/Release/netcoreapp3.1/plugins/
docker build --tag creditcoin-gateway ccgateway/bin/Release/netcoreapp3.1/.
