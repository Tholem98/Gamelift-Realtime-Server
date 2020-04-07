// Example Realtime Server Script
'use strict';

const util = require('util');

// Example override configuration
const configuration = {
    pingIntervalTime: 30000
};

// Timing mechanism used to trigger end of game session. Defines how long, in milliseconds, between each tick in the example tick loop
const tickTime = 1000;

// Defines how to long to wait in Seconds before beginning early termination check in the example tick loop
const minimumElapsedTime = 120;

let players = [];
let logicalPlayerIDs = {};

var session;                        // The Realtime server session object
var logger;                         // Log at appropriate level via .info(), .warn(), .error(), .debug()
var startTime;                      // Records the time the process started
var activePlayers = 0;              // Records the number of connected players
var onProcessStartedCalled = false; // Record if onProcessStarted has been called

// Example custom op codes for user-defined messages
// Any positive op code number can be defined here. These should match your client code.
const OP_CODE_CUSTOM_OP1 = 111;
const OP_CODE_CUSTOM_OP1_REPLY = 112;
const OP_CODE_PLAYER_ACCEPTED = 113;
const OP_CODE_DISCONNECT_NOTIFICATION = 114;

// Example groups for user defined groups
// Any positive group number can be defined here. These should match your client code.
const RED_TEAM_GROUP = 1;
const BLUE_TEAM_GROUP = 2;

// server op codes (messages server sends)
const LOGICAL_PLAYER_OP_CODE = 100;
const START_COUNTDOWN_OP_CODE = 101;
const MOVE_PLAYER_OP_CODE = 102;
const WINNER_DETERMINED_OP_CODE = 103;

// client op codes (messages client sends)
const SCENE_READY_OP_CODE = 200;
const HOP_OP_CODE = 201;

// Called when game server is initialized, passed server's object of current session
function init(rtSession) {
    session = rtSession;
    logger = session.getLogger();
}

// On Process Started is called when the process has begun and we need to perform any
// bootstrapping.  This is where the developer should insert any code to prepare
// the process to be able to host a game session, for example load some settings or set state
//
// Return true if the process has been appropriately prepared and it is okay to invoke the
// GameLift ProcessReady() call.
function onProcessStarted(args) {
    onProcessStartedCalled = true;
    logger.info("Starting process with args: " + args);
    logger.info("Ready to host games...");

    return true;
}

// Called when a new game session is started on the process
function onStartGameSession(gameSession) {
    // Complete any game session set-up

    // Set up an example tick loop to perform server initiated actions
    startTime = getTimeInS();
    tickLoop();
}

// Handle process termination if the process is being terminated by GameLift
// You do not need to call ProcessEnding here
function onProcessTerminate() {
    // Perform any clean up
    session.processEnding();
}

// Return true if the process is healthy
function onHealthCheck() {
    return true;
}

// On Player Connect is called when a player has passed initial validation
// Return true if player should connect, false to reject
function onPlayerConnect(connectMsg) {
    // Perform any validation needed for connectMsg.payload, connectMsg.peerId
    return true;
}

// Called when a Player is accepted into the game
function onPlayerAccepted(player) {
    // This player was accepted -- let's send them a message
    players.push(player.peerId);
    const playerLessMe = players.splice(players.indexOf(player.peerId),1);
    const msg = session.newTextGameMessage(OP_CODE_PLAYER_ACCEPTED,player.peerId,
                                             "Peer " + player.peerId + " accepted");
    // for(let index = 0;index<playerLessMe.length;index++){
        // session.sendReliableMessage(msg,player.peerId);
    // }
    session.getPlayers().forEach((thisPlayer, playerId) => {
        if (playerId != player.peerId) {
            session.sendReliableMessage(msg,thisPlayer.peerId);
        }
    });

    activePlayers++;

    session.getLogger().info("[app] onPlayerAccepted: new contents of players array = " + players.toString());

    let logicalID = players.length - 1;
    session.getLogger().info("[app] onPlayerAccepted: logical ID = " + logicalID);

    logicalPlayerIDs[player.peerId] = logicalID;
    session.getLogger().info("[app] onPlayerAccepted: logicalPlayerIDs array = " + logicalPlayerIDs.toString());

    // SendStringToClient(players.splice(players.indexOf(player.peerId),1), LOGICAL_PLAYER_OP_CODE, logicalID.toString());

}

// On Player Disconnect is called when a player has left or been forcibly terminated
// Is only called for players that actually connected to the server and not those rejected by validation
// This is called before the player is removed from the player list
function onPlayerDisconnect(peerId) {
    // send a message to each remaining player letting them know about the disconnect
    const outMessage = session.newTextGameMessage(OP_CODE_DISCONNECT_NOTIFICATION,
                                                session.getServerId(),
                                                "Peer " + peerId + " disconnected");
    session.getPlayers().forEach((player, playerId) => {
        if (playerId != peerId) {
            session.sendReliableMessage(outMessage, peerId);
        }
    });
    activePlayers--;
}

function SendStringToClient(peerIds, opCode, stringToSend) {
    session.getLogger().info("[app] SendStringToClient: peerIds = " + peerIds.toString() + " opCode = " + opCode + " stringToSend = " + stringToSend);

    let gameMessage = session.newTextGameMessage(opCode, session.getServerId(), stringToSend);
    let peerArrayLen = peerIds.length;
    session.getLogger().info("[app] peerArrayLen "+ peerArrayLen.toString())
    // for (let index = 0; index < peerArrayLen; index++) {
    //     session.getLogger().info("[app] SendStringToClient: sendMessageT " + gameMessage.toString() + "peerID: " + peerIds[index].toString());
    //     session.sendMessage(gameMessage, peerIds[index]);
    // };



}

// Handle a message to the server
function onMessage(gameMessage) {
    session.getLogger().info("[app] onMessage(gameMessage): ");
    session.getLogger().info(util.inspect(gameMessage));

    // sender 0 is server so we don't process them 
    if (gameMessage.sender != 0) {
        let logicalSender = logicalPlayerIDs[gameMessage.sender];

        switch (gameMessage.opCode) {
            case SCENE_READY_OP_CODE:
                playerReady[logicalSender] = true;
                // have both players signaled they are ready? If so, ready to go
                if (playerReady[0] === true && playerReady[1] === true) {
                    session.sendMessage("gameMessage", logicalSender);
                }
                break;

            case HOP_OP_CODE:
                // ProcessHop(logicalSender);
                session.getLogger().info("[app] Shoot");
                // SendStringToClient([players], HOP_OP_CODE, gameMessage.sender.toString());
                let gameMessageSend = session.newTextGameMessage(gameMessage.opCode, session.getServerId(), gameMessage.sender.toString());
                session.getPlayers().forEach((thisPlayer, playerId) => {
                    if (playerId != gameMessage.sender) {
                    session.getLogger().info("[app] SendStringToClient: sendMessageT " + gameMessageSend.toString() + "peerID: " + gameMessage.sender.toString());
            
                        session.sendMessage(gameMessageSend,thisPlayer.peerId);
                    }
                });

                break;

            default:
                session.getLogger().info("[warning] Unrecognized opCode in gameMessage");
                session.sendMessage("gameMessage", logicalSender);
        };
    }
}

// Return true if the send should be allowed
function onSendToPlayer(gameMessage) {
    // This example rejects any payloads containing "Reject"
    return (!gameMessage.getPayloadAsText().includes("Reject"));
}

// Return true if the send to group should be allowed
// Use gameMessage.getPayloadAsText() to get the message contents
function onSendToGroup(gameMessage) {
    return true;
}

// Return true if the player is allowed to join the group
function onPlayerJoinGroup(groupId, peerId) {
    return true;
}

// Return true if the player is allowed to leave the group
function onPlayerLeaveGroup(groupId, peerId) {
    return true;
}

// A simple tick loop example
// Checks to see if a minimum amount of time has passed before seeing if the game has ended
async function tickLoop() {
    const elapsedTime = getTimeInS() - startTime;
    logger.info("Tick... " + elapsedTime + " activePlayers: " + activePlayers);

    // In Tick loop - see if all players have left early after a minimum period of time has passed
    // Call processEnding() to terminate the process and quit
    if ( (activePlayers == 0) && (elapsedTime > minimumElapsedTime)) {
        logger.info("All players disconnected. Ending game");
        const outcome = await session.processEnding();
        logger.info("Completed process ending with: " + outcome);
        // process.exit(0);
    }
    else {
        setTimeout(tickLoop, tickTime);
    }
}

// Calculates the current time in seconds
function getTimeInS() {
    return Math.round(new Date().getTime()/1000);
}

exports.ssExports = {
    configuration: configuration,
    init: init,
    onProcessStarted: onProcessStarted,
    onMessage: onMessage,
    onPlayerConnect: onPlayerConnect,
    onPlayerAccepted: onPlayerAccepted,
    onPlayerDisconnect: onPlayerDisconnect,
    onSendToPlayer: onSendToPlayer,
    onSendToGroup: onSendToGroup,
    onPlayerJoinGroup: onPlayerJoinGroup,
    onPlayerLeaveGroup: onPlayerLeaveGroup,
    onStartGameSession: onStartGameSession,
    onProcessTerminate: onProcessTerminate,
    onHealthCheck: onHealthCheck
};