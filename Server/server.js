const WebSocketServer = require('ws').WebSocketServer
const crypto = require('crypto');
const port = 7778
const wss = new WebSocketServer({ host: '0.0.0.0', port });


const connections = {}
let pool = []
const games = {}
wss.on('listening', () => {
    console.log("Listening on port", port);
})

wss.on('connection', function connection(ws) {
    ws.peerid = crypto.randomUUID()
    connections[ws.peerid] = ws
    pool.push(ws.peerid)
    if (pool.length == 2) {
        const [hostid, clientid] = pool
        const hostWs = connections[hostid]
        const clientWs = connections[clientid]
        games[hostid] = clientid
        hostWs.send(JSON.stringify({
            event: 'becomeHost',
            clientid
        }))
        clientWs.send(JSON.stringify({
            event: 'becomeClient',
            hostid
        }))
        pool = []
    }

    ws.on('error', console.error);
    ws.on('message', function message(data) {
        console.log('received: %s', data);
        data = JSON.parse(data)
        if (data.event == 'offer') {
            const hostid = ws.peerid
            const clientid = games[hostid]
            const clientWs = connections[clientid]
            // const clientWs = connections[data.clientid]
            clientWs.send(JSON.stringify({
                event: "offer",
                offer: data.offer,
                hostid: ws.peerid
            }))
        }
        if (data.event == 'answer') {
            const hostWs = connections[data.hostid]
            hostWs.send(JSON.stringify({
                event: "answer",
                answer: data.answer,
                clientid: ws.peerid
            }))
        }
    });

    ws.on("close", () => {
        delete connections[ws.peerid]
    })

    //   ws.send("potatis")

});