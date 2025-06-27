# CapsLockComm

This is a tool that allows communication using the cap/num/scroll lock keys. 
This is useful in an environment when other transfer methods are blocked. 
For example, when copy & paste is disabled over a RDP session. This requires the tool to be present on both hosts, so obviously this will not work if you can't get the tool on to one of the first hosts.

```
Warning! During transfers do not move the sending or receiving windows as this can cause data errors.
```

## Protocol

I've played around with a few schemes to maximise speed and reliability. For speed the best option was to have a fix symbol length that the receiver would synchronise to and then the data could be encoded as a 3 bit number using the cap/num/scroll lock keys. Even with the sender and receiver running on the same host (ideal conditions) 20-40ms symbol lengths was the best I could achieve and still be reliable. I suspect the reliability would be even poorer over an RDP session. So I settled on an interactive protocol which had slightly lower maximum performance but conceptually has much better reliability. The bitrate will vary based on the connection. It works as follows:

### Layer 1 - Data Layer

The Caps Lock key acts as a `message ready` flag. While the num lock acts as a `bit 0` and scroll lock as `bit 1` flag. With an array of bits the sender takes the first 2 bits and turns on num/scroll lock to encode the bits as shown below:

- 00 - Num OFF & Scroll OFF
- 01 - Num OFF & Scroll ON
- 10 - Num ON & Scroll OFF
- 11 - Num ON & Scroll ON

Once these have been encoded caps lock is turned on to indicate they are ready to be read by the receiver.

The receive detects caps lock is on and reads the Num and Scroll lock key states. To indicate the values have been read the receiver turns off caps lock. (Due to this read confirmation this only works for a single reader & sender pair. A scheme where there is 1 sender and multiple readers would not work using this method)

The sender dectects that caps lock is disabled and the process is repeated for the next 2 bits.

### Layer 2 - Message Layer

The next layer up is the message layer. This is used by the sender and receiver to send an arbitary length message. 
The sender first sends the message length as a 32 bit encoded number followed by the data.

- ADD ERROR DETECTION AND/OR CORRECTION AT THIS LEVEL IN THE FUTURE. (THIS LAYER SHOULD BE RELIABLE)

- Error correction done with ReedSolomon package

### Layer 3 - Transfer Layer

This layer allows for different types of data to be transfered between the sender and the receiver. It starts with 4 bits to represent the message type followed by the message data.

Message types include: string, file, and bits.



## Dependencies
dotnet add package ReedSolomon

## Build 
```
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true /p:ReadyToRun=false /p:DebugType=none /p:EnableCompressionInSingleFile=true /p:PublishTrimmed=true
```


Then exe file output to `bin/Release/net8.0-windows/win-x64/publish`