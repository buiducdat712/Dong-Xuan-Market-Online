const connection = new signalR.HubConnectionBuilder()
    .withUrl("/chatHub")
    .build();

connection.start().catch(err => console.error("SignalR connection error: ", err));

function toggleChat() {
    const chatWindow = document.getElementById('chatWindow');
    chatWindow.style.display = chatWindow.style.display === 'none' ? 'block' : 'none';
    if (chatWindow.style.display === 'block') {
        loadFriendsList();
    }
}

function startChat(friendId, friendName) {
    document.getElementById('chatFriendName').innerText = 'Chat với ' + friendName;
    document.getElementById('chatFriendId').value = friendId;
    loadChatHistory(friendId);
}

function loadChatHistory(friendId) {
    fetch(`/Chat/ChatWith?friendId=${friendId}`)
        .then(response => response.text())
        .then(html => {
            document.getElementById('chatBody').innerHTML = html;
            scrollToBottom();
        })
        .catch(error => console.error('Error loading chat history:', error));
}

function sendMessage() {
    const chatInput = document.getElementById('chatInput');
    const message = chatInput.value.trim();
    const friendId = document.getElementById('chatFriendId').value;

    if (message !== '' && friendId) {
        fetch('/Chat/SendMessage', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ friendId: friendId, messageContent: message })
        })
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    chatInput.value = '';
                    loadChatHistory(friendId);
                } else {
                    console.error('Error sending message.');
                }
            })
            .catch(err => console.error("SendMessage error: ", err));
    }
}

connection.on("ReceiveMessage", (senderId, message, timeSent) => {
    const currentFriendId = document.getElementById('chatFriendId').value;
    if (senderId === currentFriendId) {
        loadChatHistory(currentFriendId);
    }
});

function scrollToBottom() {
    const chatBody = document.getElementById('chatBody');
    chatBody.scrollTop = chatBody.scrollHeight;
}
function loadFriendsList() {
    fetch('/Chat/GetFriendsList')
        .then(response => response.json())
        .then(data => {
            const friendsList = document.getElementById('friendsList');
            friendsList.innerHTML = '';
            data.forEach(friend => {
                const li = document.createElement('li');
                li.className = 'list-group-item';
                li.innerHTML = `<a href="#" onclick="startChat('${friend.id}', '${friend.userName}')">${friend.userName}</a>`;
                friendsList.appendChild(li);
            });
        })
        .catch(error => console.error('Error loading friends list:', error));
}
document.getElementById('searchFriends').addEventListener('keyup', filterFriends);

function filterFriends() {
    var input = document.getElementById('searchFriends').value.toLowerCase();
    var listItems = document.querySelectorAll('#friendsList li');

    listItems.forEach(function (item) {
        if (item.textContent.toLowerCase().includes(input)) {
            item.style.display = '';
        } else {
            item.style.display = 'none';
        }
    });
}

document.addEventListener("DOMContentLoaded", function () {
    loadFriendsList();
});