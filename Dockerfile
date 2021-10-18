FROM ubuntu:bionic AS builder 

WORKDIR gateway

COPY . .

RUN apt-get update
RUN apt-get install -y wget apt-transport-https
RUN wget -q https://packages.microsoft.com/config/ubuntu/18.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
RUN dpkg -i packages-microsoft-prod.deb
RUN apt-get update
RUN rm packages-microsoft-prod.deb
RUN apt-get install -y dotnet-sdk-3.1
RUN dotnet restore
RUN dotnet publish --configuration Release
RUN ls

FROM ubuntu:bionic AS runtime
WORKDIR gateway
RUN apt-get update
RUN apt-get install -y wget apt-transport-https
RUN wget -q https://packages.microsoft.com/config/ubuntu/18.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
RUN dpkg -i packages-microsoft-prod.deb
RUN rm packages-microsoft-prod.deb
RUN apt-get update
RUN apt-get install -y aspnetcore-runtime-3.1
COPY --from=builder /gateway/ccgateway/bin/Release/netcoreapp3.1/publish .
COPY --from=builder /gateway/ccgateway/plugins/ ./plugins
ENTRYPOINT ./ccgateway
