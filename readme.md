<div align="center">

|⚠️| The Sawtooth implementation of Creditcoin has been replaced by a Substate-based version. Please see https://medium.com/creditcoin-foundation and https://github.com/gluwa/creditcoin for more information. |
|-|-|

</div>

---

# Gluwa Creditcoin


## What is Creditcoin?

Creditcoin is a network that enables cross-blockchain credit transaction and credit history building. Creditcoin uses blockchain technology to ensure the objectivity of its credit transaction history: each transaction on the network is distributed and verified by the network.

The Creditcoin protocol was created by Gluwa. Gluwa Creditcoin is the official implementation of the Creditcoin protocol by Gluwa.

For more information, see https://creditcoin.org, or read the [original whitepaper](https://creditcoin.org/white-paper).


## Other Creditcoin Components

In order to facilitate modular updates, the Creditcoin components have been divided into several repos.

* This repo includes the Creditcoin Gateway
* [Sawtooth-Core](https://github.com/gluwa/Sawtooth-Core) contains the Creditcoin fork of Sawtooth 1.2.x and is where most future development will take place
* [Sawtooth-SDK-Rust](https://github.com/gluwa/Sawtooth-SDK-Rust) is the Gluwa fork of the Ruse Sawtooth SDK for Creditcoin 2.0
* [Creditcoin-Consensus-Rust](https://github.com/gluwa/Creditcoin-Consensus-Rust) is the home for the Rust-based version of the Consensus engine used by Creditcoin 2.0
* [Creditcoin-Processor-Rust](https://github.com/gluwa/Creditcoin-Processor-Rust) is where you can find the Rust-based Creditcoin Transaction Processor for Creditcoin 2.0
* [Creditcoin-Shared](https://github.com/gluwa/Creditcoin-Shared) has the CCCore, CCPlugin framework, and several plugins such as Bitcoin, ERC20, Ethereum and Ethless
* [Creditcoin-Client](https://github.com/gluwa/Creditcoin-Client) houses the command-line client for communicating with the Creditcoin blockchain


## License

Creditcoin is licensed under the [GNU Lesser General Public License](COPYING.LESSER) software license.

Licenses of dependencies distributed with this repository are provided under the `\DependencyLicense` directory.


## Development Process
----------------------

The master branch is regularly built and tested, but it is not guaranteed to be completely stable.
Tags are created regularly from release branches to indicate new official, stable release versions of Gluwa Creditcoin.


## Build and Test

Prerequisite: dotnet-sdk-3.1.

* Windows - See [Install .NET on Windows](https://docs.microsoft.com/en-us/dotnet/core/install/windows?tabs=netcore31) for more details.
* Ubuntu - See
[Install the .NET SDK or the .NET Runtime on Ubuntu](https://docs.microsoft.com/en-us/dotnet/core/install/linux-ubuntu)
for more details.
* For other OS options, see [Install .NET on Windows, Linux, and macOS](https://docs.microsoft.com/en-us/dotnet/core/install/)

```bash
dotnet restore
dotnet build
```

After building the plugin DLLs ("`g<type>.dll`") under `\ccgateway\plugins` should be copied into a `plugins` folder inside `ccgateway\bin\<Build Configuration>\netcoreapp3.1` (i.e. "`ccgateway\bin\debug\netcoreapp3.1\plugins`")

## Development Process

The master branch is regularly built and tested, but it is not guaranteed to be completely stable.
Tags are created regularly from release branches to indicate new official, stable release versions of Creditcoin Client.

## Contribute

See [Coding-Standards/C# Coding Standards.md](https://github.com/gluwa/Coding-Standards/blob/main/C%23%20Coding%20Standards.md) for our coding standards.

Issues and suggestions can be shared here.
