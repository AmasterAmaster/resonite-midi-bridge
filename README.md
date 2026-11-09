# Resonite MIDI Bridge

## Overview

Resonite MIDI Bridge is a standalone C# application that allows MIDI data to be streamed into Resonite in real time using WebSockets.

The bridge supports both:

- Live MIDI devices (keyboards, controllers, launchpads, mixers, etc.)
- MIDI file playback

All MIDI data is converted into a common JSON format before being transmitted, allowing Resonite worlds to process both live performances and pre-authored music using the same logic.

## Features
## Live Mode

Connect and use physical MIDI devices.

Supported features:

- MIDI Keyboard Input
- Multiple MIDI Devices
- Automatic Device Detection
- Automatic Device Reconnection
- Hot-Plug Device Support
- Real-Time Note Transmission
- WebSocket Broadcasting
- JSON-Based Messaging

Examples:

- Native Instruments Komplete Kontrol
- Launchkey Series
- Arturia KeyLab
- Akai MPK Series
- Any Windows MIDI-compatible device

## File Mode

Load MIDI files and stream playback events into Resonite.

Supported features:

- MIDI File Loading
- Playback Controls
- Pause / Resume
- Stop
- Replay
- Progress Tracking
- Duration Tracking
- Position Tracking
- Song Information Display

Supported file formats:

- .mid
- .midi

## Networking

Built-in WebSocket Server.

Features:

- Multiple Clients
- Persistent Connections
- Automatic Reconnection Handling
- Safe Concurrent Message Broadcasting
- JSON Serialization

Default endpoint:

- ws://localhost:8080/ws

## Example JSON Messages
Note On
```
JSON
{
"Type": "NoteOn",
"Channel": 0,
"Note": 60,
"Velocity": 127,
"Timestamp": 1794197227044,
"SourceDevice": "MidiFile"
}
```

Note Off
```
JSON
{
"Type": "NoteOff",
"Channel": 0,
"Note": 60,
"Velocity": 0,
"Timestamp": 1794197227167,
"SourceDevice": "MidiFile"
}
```

## Support
This is free to use, but support is appreciated. You can support me and this project through:
- https://www.patreon.com/AmasterAmaster
- https://ko-fi.com/amasteramaster

If you find bugs, definitely let me know.

You can also join the Team Inverse community here: https://discord.gg/nKYfcfBGQh

## License

MIT License

The software is provided AS IS.
