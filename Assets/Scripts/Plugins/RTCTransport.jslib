let wrtc
let dc
let wsSignaling
function JSFindMatch(matchmakingServerPtr, becomeHostPtr, becomeClientPtr) {
    if (typeof Runtime === 'undefined') {
        Runtime = {
            dynCall: dynCall,
        }
    }
    const matchmakingServer = UTF8ToString(matchmakingServerPtr)
    console.log('JSFindMatch', matchmakingServer)
    wsSignaling = new WebSocket(matchmakingServer)
    wsSignaling.onopen = () => console.log("Connected to signaling");
    wsSignaling.onclose = () => console.log("Disconnected from signaling");
    wsSignaling.onmessage = async (event) => {
        console.log('Recv message', event.data)
        const data = JSON.parse(event.data)

        if (data.event == 'becomeHost') {
            Runtime.dynCall('v', becomeHostPtr, [])
        } else if (data.event == 'becomeClient') {
            Runtime.dynCall('v', becomeClientPtr, [])
        }

    }
}

function JSClientConnect(onMessagePtr, onGameStartPtr) {
    if (typeof Runtime === 'undefined') {
        Runtime = {
            dynCall: dynCall,
        }
    }
    console.log('JSClientConnect')


    wsSignaling.onmessage = async (event) => {
        console.log('Recv message', event.data)
        const data = JSON.parse(event.data)

        if (data.event == 'offer') {
            wrtc = new RTCPeerConnection({ iceServers: [{ urls: 'stun:stun.l.google.com:19302' }] })
            await wrtc.setRemoteDescription(data.offer)
            const answer = await wrtc.createAnswer()
            await wrtc.setLocalDescription(answer)
            await new Promise((resolve) => {
                wrtc.onicegatheringstatechange = () => {
                    if (wrtc.iceGatheringState === 'complete') resolve()
                }
            })
            wrtc.ondatachannel = ({ channel }) => {
                dc = channel
                dc.onopen = () => {
                    console.log('dc open')
                    SendMessage('NetworkManager', 'CallbackOnClientConnected')
                    // Runtime.dynCall('v', onGameStartPtr, [])

                }

                dc.onmessage = ({ data }) => {
                    const array = new Uint8Array(data)
                    const arrayLength = array.length
                    const bufferPtr = _malloc(arrayLength)
                    const dataBuffer = new Uint8Array(HEAPU8.buffer, bufferPtr, arrayLength)
                    dataBuffer.set(array)
                    Runtime.dynCall('vii', onMessagePtr, [bufferPtr, arrayLength])
                    _free(bufferPtr)
                }
            }
            wsSignaling.send(
                JSON.stringify({
                    event: 'answer',
                    answer: wrtc.localDescription,
                    hostid: data.hostid,
                }),
            )
        }
    }
}
function JSClientDisconnect() {
    console.log('JSClientDisconnect')
}
function JSCLientSend(arrayPtr, offset, length) {
    // console.log('JSCLientSend')
    if (wrtc && dc.readyState == 'open') {
        const start = arrayPtr + offset
        const end = start + length
        const data = HEAPU8.buffer.slice(start, end)
        dc.send(data)
        return true
    }
}
async function JSServerStart(onMessagePtr, onClientPtr, onGameStartPtr) {
    if (typeof Runtime === 'undefined') {
        Runtime = {
            dynCall: dynCall,
        }
    }

    wsSignaling.onmessage = async (event) => {
        console.log('Recv message', event.data)
        const data = JSON.parse(event.data)

        if (data.event == 'answer') {
            await wrtc.setRemoteDescription(data.answer)
        }
    }

    wrtc = new RTCPeerConnection({ iceServers: [{ urls: 'stun:stun.l.google.com:19302' }] })
    dc = wrtc.createDataChannel('dc')
    dc.binaryType = 'arraybuffer'
    dc.onopen = () => {
        Runtime.dynCall('v', onClientPtr, [])
        console.log('dc open')
        Runtime.dynCall('v', onGameStartPtr, [])
    }

    dc.onmessage = ({ data }) => {
        const array = new Uint8Array(data)
        const arrayLength = array.length
        const bufferPtr = _malloc(arrayLength)
        const dataBuffer = new Uint8Array(HEAPU8.buffer, bufferPtr, arrayLength)
        dataBuffer.set(array)
        Runtime.dynCall('vii', onMessagePtr, [bufferPtr, arrayLength])
        _free(bufferPtr)
    }

    const offer = await wrtc.createOffer()
    await wrtc.setLocalDescription(offer)
    await new Promise((resolve) => {
        wrtc.onicegatheringstatechange = () => {
            if (wrtc.iceGatheringState === 'complete') resolve()
        }
    })
    wsSignaling.send(
        JSON.stringify({
            event: 'offer',
            offer: wrtc.localDescription,
        }),
    )
}
function JSServerDisconnect() {
    console.log('JSServerDisconnect')
}
function JSServerStop() {
    console.log('JSServerStop')
}
function JSServerSend(arrayPtr, offset, length) {
    // console.log('JSServerSend')
    if (wrtc && dc.readyState == 'open') {
        const start = arrayPtr + offset
        const end = start + length
        const data = HEAPU8.buffer.slice(start, end)
        dc.send(data)
        return true
    }
}

const RTCLib = {
    JSFindMatch,
    JSClientConnect,
    JSClientDisconnect,
    JSCLientSend,
    JSServerStart,
    JSServerDisconnect,
    JSServerStop,
    JSServerSend,
}
mergeInto(LibraryManager.library, RTCLib)
