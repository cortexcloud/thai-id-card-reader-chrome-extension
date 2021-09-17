/*
On startup, connect to the "co.cortexcloud.thai_id_card_reader" app.
*/
var port = chrome.runtime.connectNative("co.cortexcloud.thai_id_card_reader");

/*
Listen for messages from the app.
*/
port.onMessage.addListener((response) => {
  console.log("Received: " + JSON.stringify(response));
  onDisconnect();
});


console.log('Background script is started.')

function onDisconnect() {
    port.onDisconnect.addListener(function () {
        if (chrome.runtime.lastError) {
            console.log(chrome.runtime.lastError);
        }
    });
}