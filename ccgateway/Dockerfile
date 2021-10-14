FROM ubuntu:18.04 as base

WORKDIR /home/Creditcoin/Gateway

COPY . /home/Creditcoin/Gateway/

FROM base as dotnet

RUN apt-get update
RUN apt-get install -y wget apt-transport-https
RUN wget -q https://packages.microsoft.com/config/ubuntu/18.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
RUN dpkg -i packages-microsoft-prod.deb
RUN rm packages-microsoft-prod.deb
RUN apt-get update
RUN apt-get install -y dotnet-sdk-3.1

FROM dotnet as build

RUN dotnet build

RUN ln -sr /home/Creditcoin/Gateway/bin/Debug/netcoreapp3.1/ccgateway .

FROM build as gateway

CMD ["./ccgateway"]
