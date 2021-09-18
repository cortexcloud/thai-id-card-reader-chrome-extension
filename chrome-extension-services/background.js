/*
On startup, connect to the "co.cortexcloud.thai_id_card_reader" app.
*/
var host = 'co.cortexcloud.thai_id_card_reader_window_services';
var port = null;

/*
Listen for messages from the app.
*/
onConnect();
function onConnect(){
	port = chrome.runtime.connectNative(host);
	port.onMessage.addListener(onNativeMessage);
}
/*
port.onMessage.addListener((response) => {
  console.log("Received: " + JSON.stringify(response));
  onDisconnect();
});
*/

console.log('Background script is started.')
function onNativeMessage(response) {
   // alert(message);
    console.log('recieved message from native app: ' + JSON.stringify(response));
	onDisconnect();
}


function onDisconnect() {
    port.onDisconnect.addListener(function () {
        if (chrome.runtime.lastError) {
            console.log(chrome.runtime.lastError);
        }
    });
}