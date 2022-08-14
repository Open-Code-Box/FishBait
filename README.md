![Logo](FishBait_Logo.png)

# FishBait

[![Maintainability](https://api.codeclimate.com/v1/badges/954d1b30c2da8f61037e/maintainability)](https://codeclimate.com/github/Derek-R-S/Light-Reflective-Mirror/maintainability)

LRM Node / MultiCompiled

## What

FishBait is a transport FishNet Networking based on Light Reflective Mirror which relays network traffic through your own servers. This allows you to have clients host game servers and not worry about NAT/Port Forwarding, etc. There are still features I plan on adding but it still is completely stable in its current state.

## Features

* Built-in server list!
* Relay password to stop other games from stealing your precious relay!
* Relay supports connecting users without them needing to port forward!
* NAT Punchtrough (Full Cone, Restricted Cone, and Port Restricted Cone)
* Direct Connecting
* Load Balancing with multi-relay setup

## How does it work?

Light Reflective Mirror creator took a bit of a unique approach to this version and instead of using one fixed net library for the game to communicate with the standalone relay server, He instead made it use any transport! This allows you to make it work with WebSockets Ignorance(ENET), LiteNetLib, and all the others!

## Known Issues/Flaws

Disconnects from the relay will not auto-reconnect **yet**. So a dedicated host is extremely recommended! Or implement your own logic to auto-reconnect.

## Tutorials

### How to setup FishBait on an ubuntu server

**TBD**

### How to setup LRM in unity, along with basic usage

**TBD**

## Usage (Not Out Yet)

Now for the juicy part, using it. Keep in mind that this is a prototype, so if there are any problems, please report them to me. Also, PRs are always welcomed! :)

First things first, you will need:

* FishNet, Install that from Asset Store.
* Download the latest release of FishBait Unity Package and put that in your project also. Download from: [Releases](https://github.com/Derek-R-S/Light-Reflective-Mirror/releases).

#### Client Setup

Running a client is fairly straightforward, attach the FishBaitTransport script to your NetworkManager and set it as the transport in TransportManager. Put in the IP/Port of your relay server. Just follow the instructions in transpor's setup window.

 When you start a server, you can simply get the URI from the transport and use that to connect. If you wish to connect without the URI, the LightReflectiveMirror component has a public "Server ID" field which is what clients would set as the address to connect to. 

If your relay server has a password, enter it in the relayPassword field, or else you won't be able to connect. By default, the relays have the password as "Secret Auth Key".

##### Server List

Light Reflective Mirror and by proxy FishBait have a built-in room/server list if you would like to use it. To use it you need to set all the values in the 'Server Data' tab in the transport. Also if you would like to make the server show on the list, make sure "Is Public Server" is checked. Once you create a server, you can update those variables from the "UpdateRoomInfo" function on the FishBaitTransport script.

To request the server list you need a reference to the FishBaitTransport from your script and call 'RequestServerList()'. This will invoke a request to the server to update our server list. Once the response is received the field 'relayServerList' will be populated and you can get all the servers from there.

#### Server Setup

Download the latest Server release from: [Releases](https://github.com/Derek-R-S/Light-Reflective-Mirror/releases)
Make sure you have .NET Core 5.0
And all you need to do is run FishBait.exe on windows, or "dotnet FishBait.dll" on Linux!

#### Server Config

In the config.json file, there are a few fields.

AuthenticationKey - This is the key the clients need to have on their inspector. It cannot be blank.

UpdateLoopTime - The time in milliseconds between calling 'Update' on the transport

UpdateHeartbeatInterval - the amounts of update calls before sending a heartbeat. By default its 100, which if updateLoopTime is 10, means every (10 * 100 = 1000ms) it will send out a heartbeat.

## What to choose, Epic, Steam, LRM?

There are a few relay transports available for FishNet, It can often be difficult to pick one that most suits your needs. So I'll quickly go over my view on it and hopefully, it helps you make an informed decision.

### Steam

Starting with steam, steam offers a free relay with NAT punch through for anyone releasing a game on steam. This integrates into their lobby invites and also only allows connections from other users who own the game (No pirates sneaking into your servers!) and it works wonders. Steam has a well-documented SDK, a huge community, and they are active on their forums. If you plan on releasing on steam and only steam, go with this. To use the steam relay download either FishySteamworks or FishyFacepunch depending on whether you use [Steamworks.Net SDK](https://github.com/rlabrecque/Steamworks.NET) or [Facepunch.Steamworks SDK](https://github.com/Facepunch/Facepunch.Steamworks).

### Epic

Epic is a newer transport that offers NAT punch through, and relay service for free. It supports Windows, Mac, Linux, PlayStation, Xbox, Nintendo Switch, iOS, and Android, so that's pretty much all that you could ever ask for. They also have more tools such as Matchmaking, server browser, statistics, and more! This is NOT locked into only releasing on Epic Store, like how steams is. So you can release your game anywhere you want if your game uses this (as long as Epic doesn't get in legal trouble with more companies). Now onto the downsides, they have a very PITA SDK to use with a fairly small community for the C# side of things. The documentation is sub-par and severely lacking in some places, which is expected as it's fairly new. They also have Epic Account Services, which is similar to steams but like the relay, not locked into one store! With those services, you get user accounts, In-game purchases, achievements, and much more. So if you want a free relay/NAT Punchthrough server, and want to go along for the ride of EoS, this is the one. When it's out I'm gonna link it here but if you wanna get notified as soon as it gets released then head on to [FishNet's discord](https://discord.gg/fishnetworking).

### FishBait

FishBait is a self-hosted, open source, relay/NAT Punchthrough server. It's available for all platforms (PC, Mac, Linux, WebGL, Android, IOS, You name it!). It does this by supporting any of FishBait's existing transports. The game developer can decide on how they want their data sent between the server and clients. With FishBait, you are going to have to host the servers yourself. FishBait also has a Load Balancer, which allows you to even out the relay usage between multiple machines, this also allows you to have regional servers and lower latency for players living in different regions. The more powerful of a server you have, the more that FishNet node can host. With some tests (All clients relayed, none NAT punched), you could get about ~400CCU on a free oracle cloud server (A1.Flex arm ubuntu 24GB ram). Though, FishBait is still constantly being worked on and could have changes, rewrites, etc at any time. You are welcome to contribute if you find any bugs, just by opening a PR!

So, if you are more of a self-hosting person, who wants full control of your servers, or wants a relay for a platform the others don't support. Use FishBait, if you have any questions, you can ping me on [FishNet discord](https://discord.gg/fishnetworking)! :) (||NIMFER||#8982)

## Credits

### FishBait Credits

Derek - For creating Light Reflecting Mirror

FirstGearGames - For creating FishNet

You - For reading the README and checking out FishBait

### LMR Credits

Cooper - Assisted with development and made some wonderful features! He's also active in the discord to help answer questions and help with issues.

Maqsoom & JesusLuvsYooh - Both are active testers and have been testing it since I pitched the idea. They tested almost all versions of DRM and I am sure they will test the crap out of LRM!

All Mirror Transport Creators! - They made all the transports that this thing relies on! Especially the Simple Web Transport by default!

## License

[MIT](https://choosealicense.com/licenses/mit/)
