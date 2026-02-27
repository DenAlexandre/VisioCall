// WebRTC logic — communicates with C# via visiocall:// URL scheme

let pc = null;
let localStream = null;
const statusEl = document.getElementById('status');
const remoteVideo = document.getElementById('remoteVideo');
const localVideo = document.getElementById('localVideo');

const config = {
    iceServers: [
        { urls: 'stun:stun.l.google.com:19302' },
        { urls: 'stun:stun1.l.google.com:19302' }
    ]
};

// --- Message queue to avoid losing messages with location.href ---
const messageQueue = [];
let sending = false;

function sendToNative(action, data) {
    const encoded = encodeURIComponent(JSON.stringify(data));
    const url = 'visiocall://' + action + '/' + encoded;
    messageQueue.push(url);
    processQueue();
}

function processQueue() {
    if (sending || messageQueue.length === 0) return;
    sending = true;
    const url = messageQueue.shift();
    const iframe = document.createElement('iframe');
    iframe.style.display = 'none';
    iframe.src = url;
    document.body.appendChild(iframe);
    setTimeout(() => {
        iframe.remove();
        sending = false;
        processQueue();
    }, 100);
}

// --- Media ---
async function initMedia() {
    try {
        localStream = await navigator.mediaDevices.getUserMedia({
            video: { facingMode: 'user', width: { ideal: 640 }, height: { ideal: 480 } },
            audio: true
        });
        localVideo.srcObject = localStream;
        statusEl.textContent = 'Camera ready';
        return true;
    } catch (err) {
        statusEl.textContent = 'Camera error: ' + err.message;
        sendToNative('error', 'getUserMedia failed: ' + err.message);
        return false;
    }
}

// --- Peer Connection ---
function createPeerConnection() {
    pc = new RTCPeerConnection(config);

    if (localStream) {
        localStream.getTracks().forEach(track => pc.addTrack(track, localStream));
    }

    pc.ontrack = (event) => {
        statusEl.textContent = '';
        if (event.streams && event.streams[0]) {
            remoteVideo.srcObject = event.streams[0];
        } else {
            // Fallback: create a new MediaStream from the track
            let stream = remoteVideo.srcObject;
            if (!stream) {
                stream = new MediaStream();
                remoteVideo.srcObject = stream;
            }
            stream.addTrack(event.track);
        }
    };

    pc.onicecandidate = (event) => {
        if (event.candidate) {
            sendToNative('ice-candidate', {
                candidate: event.candidate.candidate,
                sdpMid: event.candidate.sdpMid,
                sdpMLineIndex: event.candidate.sdpMLineIndex
            });
        }
    };

    pc.oniceconnectionstatechange = () => {
        const state = pc.iceConnectionState;
        if (state === 'connected' || state === 'completed') {
            statusEl.textContent = '';
        } else if (state === 'disconnected' || state === 'failed') {
            statusEl.textContent = 'Connection lost';
        }
    };
}

// --- Caller: create offer ---
async function createOffer() {
    const ready = await initMedia();
    if (!ready) return;

    createPeerConnection();
    statusEl.textContent = 'Creating offer...';

    try {
        const offer = await pc.createOffer();
        await pc.setLocalDescription(offer);
        sendToNative('offer', { type: offer.type, sdp: offer.sdp });
    } catch (err) {
        sendToNative('error', 'createOffer failed: ' + err.message);
    }
}

// --- Callee: receive offer then auto-answer ---
async function receiveOffer(offer) {
    const ready = await initMedia();
    if (!ready) return;

    createPeerConnection();
    statusEl.textContent = 'Connecting...';

    try {
        await pc.setRemoteDescription(new RTCSessionDescription({
            type: offer.type,
            sdp: offer.sdp
        }));

        const answer = await pc.createAnswer();
        await pc.setLocalDescription(answer);
        sendToNative('answer', { type: answer.type, sdp: answer.sdp });
    } catch (err) {
        sendToNative('error', 'receiveOffer failed: ' + err.message);
    }
}

// --- Caller: receive answer ---
async function receiveAnswer(answer) {
    if (!pc) return;
    try {
        await pc.setRemoteDescription(new RTCSessionDescription({
            type: answer.type,
            sdp: answer.sdp
        }));
    } catch (err) {
        sendToNative('error', 'receiveAnswer failed: ' + err.message);
    }
}

// --- Both: add ICE candidate ---
async function receiveIceCandidate(candidate) {
    if (!pc) return;
    try {
        await pc.addIceCandidate(new RTCIceCandidate({
            candidate: candidate.candidate,
            sdpMid: candidate.sdpMid,
            sdpMLineIndex: candidate.sdpMLineIndex
        }));
    } catch (err) {
        sendToNative('error', 'addIceCandidate failed: ' + err.message);
    }
}

function toggleMute() {
    if (!localStream) return;
    const audioTrack = localStream.getAudioTracks()[0];
    if (audioTrack) {
        audioTrack.enabled = !audioTrack.enabled;
    }
}

function toggleCamera() {
    if (!localStream) return;
    const videoTrack = localStream.getVideoTracks()[0];
    if (videoTrack) {
        videoTrack.enabled = !videoTrack.enabled;
    }
}

function closeConnection() {
    if (pc) {
        pc.close();
        pc = null;
    }
    if (localStream) {
        localStream.getTracks().forEach(track => track.stop());
        localStream = null;
    }
    remoteVideo.srcObject = null;
    localVideo.srcObject = null;
}
